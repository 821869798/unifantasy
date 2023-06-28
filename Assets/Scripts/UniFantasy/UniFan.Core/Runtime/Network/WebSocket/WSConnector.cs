using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace UniFan.Network
{
    public class WSConnector : ISocket
    {
        /// <summary>
        /// Socket对象
        /// </summary>
        protected ClientWebSocket Socket { get; set; }

        protected Uri Uri { get; set; }

        /// <summary>
        /// Socket名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Socket状态
        /// </summary>
        public SocketStatus Status
        {
            get
            {
                return status;
            }
            protected set
            {
                status = value;
            }
        }
        private SocketStatus status;

        public WebSocketMessageType MessageType { set; get; } = WebSocketMessageType.Binary;

        /// <summary>
        /// 接受消息的buff size
        /// </summary>
        public virtual int ReceiveBuffSize { get; set; }

        /// <summary>
        /// 发送的缓存数据
        /// </summary>
        protected RingBuffer PendingSentBuffer { get; set; }

        protected Queue<int> sendBuffSizes { get; }

        internal byte[] SentState { private set; get; }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected => Status == SocketStatus.Establish;

        protected long sentBytes;
        public long SentBytes => sentBytes;

        protected long receiveBytes;
        public long ReceiveBytes => receiveBytes;

        /// <summary>
        /// 开始连接的超时时间
        /// </summary>
        protected virtual float CfgConnectTimeout => 5f;

        /// <summary>
        /// 是否需要去关闭
        /// </summary>
        protected bool needToClose;

        ///// <summary>
        ///// 是否发送消息中
        ///// </summary>
        //protected int sendingCount;

        /// <summary>
        /// 加锁
        /// </summary>
        protected object SyncRoot { get; private set; }

        public event Action<ISocket> OnConnecting;
        public event Action<ISocket> OnConnected;
        public event Action<ISocket, Exception> OnClosed;
        public event Action<ISocket, ArraySegment<byte>> OnMessage;
        public event Action<ISocket, Exception> OnError;

        public WSConnector(string name, Uri uri, int receiveBuffSize = 65536)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Param [name] cannot be null or empty");
            }
            Name = name;
            this.Uri = uri;
            this.ReceiveBuffSize = receiveBuffSize;
            this.sendBuffSizes = new Queue<int>();
            SyncRoot = new object();
            Initial();
        }

        protected virtual void Initial()
        {
            PendingSentBuffer = new RingBuffer(65536);
            SentState = new byte[2048];
        }

        public void ChangeIpEndPoint(IPEndPoint ipEndPoint)
        {
            throw new NotSupportedException("WebSocket Client no supported ChangeIpEndPoint");
        }

        public virtual void ChangeIpEndPoint(Uri uri)
        {
            this.Uri = uri;
        }

        public virtual async void Connect(Action<ConnectResults, Exception> callback = null)
        {

            if (Socket != null)
            {
                Socket.Dispose();
            }
            Socket = new ClientWebSocket();
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(CfgConnectTimeout))) // 设置超时时间为10秒
            {
                try
                {
                    Status = SocketStatus.Connecting;
                    TriggerOnConnecting();

                    //start connect
                    await Socket.ConnectAsync(Uri, cts.Token);

                    Status = SocketStatus.Establish;
                    TriggerConnectCallback(callback, null);
                    TriggerOnConnected();
                }
                catch (OperationCanceledException)
                {
                    // 连接超时
                    var timeEx = new TimeoutException("Socket connect timeout");
                    TriggerConnectCallback(callback, timeEx);
                    Close(timeEx, Socket);
                    return;
                }
                catch (Exception ex)
                {
                    // 其他错误
                    TriggerConnectCallback(callback, ex);
                    Close(ex, Socket);
                    return;
                }
            }

            ReceiveMessagesAsync(Socket);

        }

        protected void TriggerConnectCallback(Action<ConnectResults, Exception> callback, Exception ex)
        {
            if (callback != null)
            {
                try
                {
                    callback((ex == null) ? ConnectResults.Success : ConnectResults.Faild, ex);
                }
                catch (Exception exception)
                {
                    TriggerError(exception);
                }
            }
        }

        public virtual async void ReceiveMessagesAsync(ClientWebSocket webSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[this.ReceiveBuffSize]);

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                if (!this.Connected || needToClose)
                {
                    return;
                }
                try
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // 出现其他错误
                    Close(ex, webSocket);
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // 服务器请求关闭连接
                    Disconnect();
                    return;
                }

                if (result.MessageType == MessageType)
                {
                    var recvData = new ArraySegment<byte>(buffer.Array, buffer.Offset, result.Count);
                    OnReceiveBytes(ref recvData);
                }
            }
        }

        protected virtual void OnReceiveBytes(ref ArraySegment<byte> receive)
        {
            TriggerOnMessage(ref receive);
        }


        public void OnUpdate(float deltaTime, float unsacaleTime)
        {

        }

        public SendResults Send(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return SendResults.Success;
            }
            return Send(data, 0, data.Length);
        }

        public SendResults Send(byte[] data, int offset)
        {
            if (data == null || data.Length == 0)
            {
                return SendResults.Success;
            }
            return Send(data, offset, data.Length - offset);
        }

        public SendResults Send(byte[] data, int offset, int count)
        {
            if (data == null || data.Length == 0 || count <= 0)
            {
                return SendResults.Success;
            }
            lock (SyncRoot)
            {
                if (!Connected || needToClose)
                {
                    return SendResults.Faild;
                }
                if (PendingSentBuffer.CanWrite(count))
                {
                    PendingSentBuffer.Write(data, offset, count);
                    bool sending = sendBuffSizes.Count > 0;
                    sendBuffSizes.Enqueue(count);
                    if (sending)
                    {
                        return SendResults.Pending;
                    }
                    SendMessageAsync(CancellationToken.None);
                    return SendResults.Success;
                }
            }

            Close(new RuntimeException("Socket[" + Name + "] Send Buffer Full Exception"), Socket);
            return SendResults.Faild;
        }

        protected virtual async void SendMessageAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (sendBuffSizes.Count > 0 && this.Connected && !needToClose)
                {
                    if (this.Connected && !needToClose)
                    {
                        int buffsize = sendBuffSizes.Peek();
                        bool endMessage = buffsize <= SentState.Length;
                        if (endMessage)
                        {
                            //一次性发完
                            int read = PendingSentBuffer.Read(SentState, 0, buffsize);
                            var buffer = new ArraySegment<byte>(SentState, 0, read);
                            await this.Socket.SendAsync(buffer, this.MessageType, endMessage, cancellationToken);
                        }
                        else
                        {
                            //需要多次发送
                            int lastSendSize = buffsize;
                            while (lastSendSize > 0)
                            {
                                int sentSize = Math.Min(SentState.Length, lastSendSize);
                                int read = PendingSentBuffer.Read(SentState, 0, sentSize);
                                var buffer = new ArraySegment<byte>(SentState, 0, read);
                                lastSendSize -= read;
                                endMessage = lastSendSize <= 0;
                                await this.Socket.SendAsync(buffer, this.MessageType, endMessage, cancellationToken);
                            }

                        }
                        Interlocked.Add(ref sentBytes, buffsize);
                    }
                    sendBuffSizes.Dequeue();
                }
            }
            catch (Exception ex2)
            {
                Close(ex2, Socket);
                return;
            }

            PendingClosing();
        }

        protected virtual void PendingClosing()
        {
            if (needToClose)
            {
                Close(null, Socket);
            }
        }

        public CloseResults Disconnect(Exception exception = null)
        {
            return Close(exception, Socket);
        }


        public void Dispose()
        {
            Disconnect();
            Socket?.Dispose();
        }


        protected virtual CloseResults Close(Exception exception, ClientWebSocket socket)
        {
            lock (SyncRoot)
            {
                if (socket != Socket)
                {
                    return CloseResults.Closed;
                }
                if (Status == SocketStatus.Closing)
                {
                    return CloseResults.InClosing;
                }
                if (Status == SocketStatus.Initial || Status == SocketStatus.Closed || Socket == null)
                {
                    return CloseResults.Closed;
                }
                if (exception == null && sendBuffSizes.Count > 0)
                {
                    needToClose = true;
                    return CloseResults.Pending;
                }
                Status = SocketStatus.Closing;
            }
            Task.Run(async () =>
            {
                try
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                finally
                {
                    Status = SocketStatus.Closed;
                    Reset();
                    TriggerOnClosed(exception);
                }
            });
            return CloseResults.InClosing;
        }

        protected virtual void TriggerOnConnected()
        {
            if (this.OnConnected != null)
            {
                try
                {
                    this.OnConnected(this);
                }
                catch (Exception ex)
                {
                    TriggerError(ex);
                }
            }
        }

        protected virtual void TriggerOnConnecting()
        {
            if (this.OnConnecting != null)
            {
                try
                {
                    this.OnConnecting(this);
                }
                catch (Exception ex)
                {
                    TriggerError(ex);
                }
            }
        }

        protected virtual void TriggerOnMessage(ref ArraySegment<byte> receive)
        {
            if (this.OnMessage != null)
            {
                try
                {
                    this.OnMessage(this, receive);
                }
                catch (Exception ex)
                {
                    TriggerError(ex);
                }
            }
        }

        protected virtual void TriggerOnClosed(Exception exception)
        {
            if (this.OnClosed != null)
            {
                try
                {
                    this.OnClosed(this, exception);
                }
                catch (Exception ex)
                {
                    TriggerError(ex);
                }
            }
        }

        protected virtual void TriggerError(Exception exception)
        {
            if (this.OnError != null)
            {
                try
                {
                    this.OnError(this, exception);
                }
                catch (Exception)
                {
                }
            }
        }


        protected void Reset()
        {
            lock (SyncRoot)
            {
                OnReset();
            }
        }

        protected virtual void OnReset()
        {
            Socket = null;
            PendingSentBuffer.Flush();
            sendBuffSizes.Clear();
            needToClose = false;
            sentBytes = 0L;
            receiveBytes = 0L;
        }


    }
}

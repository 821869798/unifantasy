using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UniFan.Network
{
    using Socket = System.Net.Sockets.Socket;
    public abstract class SocketConnector : ISocket
    {

        protected class SocketReceiveStates
        {
            internal Socket Socket { get; set; }

            internal byte[] Receive { private set; get; }

            internal SocketReceiveStates(int cap)
            {
                Receive = new byte[cap];
            }
        }

        protected class SocketSentStates
        {
            internal byte[] Sent { private set; get; }

            internal Socket Socket { get; set; }

            internal int BufferSize { get; set; }

            internal int SentSize { get; set; }

            internal SocketSentStates(int cap)
            {
                Sent = new byte[cap];
            }
        }

        /// <summary>
        /// Socket对象
        /// </summary>
        protected Socket Socket { get; set; }

        /// <summary>
        /// Socket名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        protected IPEndPoint LastIpEndPort { get; set; }

        /// <summary>
        /// 网络协议
        /// </summary>
        protected ProtocolType ProtocolType { get; set; }

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
        private volatile SocketStatus status;

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected => Status == SocketStatus.Establish;


        /// <summary>
        /// 发送的缓存数据
        /// </summary>
        protected RingBuffer PendingSentBuffer { get; set; }

        /// <summary>
        /// 接受状态
        /// </summary>
        protected SocketReceiveStates ReceiveDataStates { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        protected SocketSentStates SentDataStates { get; set; }

        protected virtual SocketShutdown SocketShutdown => SocketShutdown.Both;

        /// <summary>
        /// 加锁
        /// </summary>
        protected object SyncRoot { get; private set; }

        public event Action<ISocket> OnConnecting;

        public event Action<ISocket> OnConnected;

        public event Action<ISocket, Exception> OnClosed;

        public event Action<ISocket, ArraySegment<byte>> OnMessage;

        public event Action<ISocket, Exception> OnError;

        protected float timestamp;

        protected virtual float CfgConnectTimeout => 5f;

        protected bool sending;

        protected bool needToClose;

        protected long sentBytes;

        protected long receiveBytes;

        public long SentBytes => sentBytes;

        public long ReceiveBytes => receiveBytes;

        public SocketConnector(string name, IPEndPoint ipEndPoint, ProtocolType protocolType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Param [name] cannot be null or empty");
            }
            Name = name;
            LastIpEndPort = ipEndPoint;
            ProtocolType = protocolType;
            Status = SocketStatus.Initial;
            SyncRoot = new object();
            Initial();
            Reset();
        }

        protected virtual void Initial()
        {
            PendingSentBuffer = new RingBuffer(65536);
            ReceiveDataStates = new SocketReceiveStates(4096);
            SentDataStates = new SocketSentStates(4096);
        }

        public virtual void ChangeIpEndPoint(IPEndPoint ipEndPoint)
        {
            if (ipEndPoint == null)
            {
                throw new ArgumentException("Params [ipEndPoint] cannot be null", "ipEndPoint");
            }
            LastIpEndPort = ipEndPoint;
        }

        public void ChangeUri(Uri uri)
        {
            throw new NotSupportedException("Normal Socket Client no supported ChangeUri");
        }

        public virtual async Task<SocketConnectResult> Connect()
        {
            if (LastIpEndPort == null)
            {
                throw new InvalidOperationException("Current IpEndPort is null,can't to connect,please call ChangeIpEndPoint to set");
            }
            if (Status == SocketStatus.Connecting)
            {
                return new SocketConnectResult
                {
                    Result = ConnectResults.Faild,
                    Exception = new InvalidOperationException("Abnormal status [" + Status + "]")
                };
            }

            lock (SyncRoot)
            {
                if (Status != SocketStatus.Initial && Status != SocketStatus.Closed)
                {
                    throw new InvalidOperationException("Current statu [" + status + "] can not connect");
                }
                Status = SocketStatus.Connecting;
            }

            Reset();
            Socket = MakeSocket();

            try
            {
                Task taskConnect = Socket.ConnectAsync(LastIpEndPort);
                CancellationTokenSource timeoutCts = new();
                Task taskTimeout = Task.Delay((int)(CfgConnectTimeout * 1000), timeoutCts.Token);
                Task taskComplete = await Task.WhenAny(taskConnect, taskTimeout);
                if (taskComplete == taskConnect)
                {
                    //连接完成
                    timeoutCts.Cancel();
                    timeoutCts.Dispose();
                    if (Socket.Connected)
                    {
                        lock (SyncRoot)
                        {
                            if (Status == SocketStatus.Connecting)
                            {
                                Status = SocketStatus.Establish;
                                StartReceive(Socket);
                                return new SocketConnectResult
                                {
                                    Result = ConnectResults.Success,
                                    Exception = null
                                };
                            }
                        }
                    }

                }
                else
                {
                    // 链接超时
                    var exp = new TimeoutException("Socket connect timeout");
                    Close(exp, Socket);
                    return new SocketConnectResult
                    {
                        Result = ConnectResults.Faild,
                        Exception = exp
                    };
                }
            }
            catch (Exception ex)
            {
                Close(ex, Socket);
                return new SocketConnectResult
                {
                    Result = ConnectResults.Faild,
                    Exception = ex
                };
            }

            return new SocketConnectResult
            {
                Result = ConnectResults.Faild,
                Exception = new InvalidOperationException("Abnormal status [" + Status + "]")
            };
        }

        public virtual void OnUpdate(float deltaTime, float unsacaleTime)
        {
            timestamp += unsacaleTime;
        }

        public SendResults Send(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return SendResults.Ignore;
            }
            return Send(data, 0, data.Length);
        }

        public virtual SendResults Send(byte[] data, int offset, int count)
        {
            if (data == null || data.Length == 0 || count <= 0)
            {
                return SendResults.Ignore;
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
                    return sending ? SendResults.Pending : StartSend();
                }
            }
            //TriggerOnBufferFull(data);
            Close(new RuntimeException("Socket[" + Name + "] Send Buffer Full Exception"), Socket);
            return SendResults.Faild;
        }

        public virtual SendResults Send(ReadOnlySpan<byte> data)
        {
            if (data.Length <= 0)
            {
                return SendResults.Ignore;
            }
            lock (SyncRoot)
            {
                if (!Connected || needToClose)
                {
                    return SendResults.Faild;
                }
                if (PendingSentBuffer.CanWrite(data.Length))
                {
                    PendingSentBuffer.Write(data);
                    return sending ? SendResults.Pending : StartSend();
                }
            }
            //TriggerOnBufferFull(data);
            Close(new RuntimeException("Socket[" + Name + "] Send Buffer Full Exception"), Socket);
            return SendResults.Faild;
        }

        protected virtual SendResults StartSend()
        {
            try
            {
                int read = PendingSentBuffer.Read(SentDataStates.Sent, 0, SentDataStates.Sent.Length);
                if (read > 0)
                {
                    sending = true;
                    SentDataStates.Socket = Socket;
                    SentDataStates.BufferSize = read;
                    SentDataStates.SentSize = 0;
                    Socket.BeginSend(SentDataStates.Sent, 0, read, SocketFlags.None, EndSend, SentDataStates);
                }
            }
            catch (Exception ex)
            {
                Close(ex, Socket);
                return SendResults.Faild;
            }
            return SendResults.Success;
        }

        protected virtual void EndSend(IAsyncResult result)
        {
            SocketSentStates state = (SocketSentStates)result.AsyncState;
            try
            {
                int sent = state.Socket.EndSend(result);
                sentBytes += sent;
                state.SentSize += sent;
                if (state.SentSize < state.BufferSize)
                {
                    state.Socket.BeginSend(state.Sent, state.SentSize, state.BufferSize - state.SentSize, SocketFlags.None, EndSend, state);
                    return;
                }
            }
            catch (Exception ex2)
            {
                Close(ex2, state.Socket);
                return;
            }
            lock (SyncRoot)
            {
                int read = PendingSentBuffer.Read(state.Sent, 0, state.Sent.Length);
                if (read <= 0)
                {
                    sending = false;
                    //TriggerOnBufferDrain();
                    PendingClosing(state.Socket);
                    return;
                }
                state.SentSize = 0;
                state.BufferSize = read;
            }
            try
            {
                state.Socket.BeginSend(state.Sent, 0, state.BufferSize, SocketFlags.None, EndSend, state);
            }
            catch (Exception ex)
            {
                Close(ex, state.Socket);
            }
        }

        protected virtual void PendingClosing(Socket socket)
        {
            if (needToClose)
            {
                Close(null, socket);
            }
        }

        public virtual void Dispose()
        {
            Disconnect();
            Socket?.Dispose();
        }

        public CloseResults Disconnect(Exception exception = null)
        {
            return Close(exception, Socket);
        }


        protected virtual CloseResults Close(Exception exception, Socket socket)
        {
            lock (SyncRoot)
            {
                if (socket != Socket)
                {
                    return CloseResults.Closed;
                }
                if (Status == SocketStatus.Initial || Status == SocketStatus.Closed || Socket == null)
                {
                    return CloseResults.Closed;
                }
                if (exception == null && sending)
                {
                    needToClose = true;
                    return CloseResults.Pending;
                }
            }
            try
            {
                if (Socket.Connected)
                {
                    Socket.Shutdown(SocketShutdown);
                }
            }
            catch (Exception ex)
            {
                TriggerError(ex);
            }
            finally
            {
                try
                {
                    Socket.Close();
                }
                finally
                {
                    Status = SocketStatus.Closed;
                    Reset();
                    TriggerOnClosed(exception);
                }
            }
            return CloseResults.Closed;
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
            sending = false;
            needToClose = false;
            sentBytes = 0L;
            receiveBytes = 0L;
        }

        protected void StartReceive(Socket socket)
        {
            if (Status != SocketStatus.Establish)
            {
                Close(new InvalidOperationException("Abnormal status [" + Status + "]"), socket);
                return;
            }
            ReceiveDataStates.Socket = socket;
            BeginReceive(ReceiveDataStates);
        }

        protected virtual void BeginReceive(SocketReceiveStates states)
        {
            try
            {
                states.Socket.BeginReceive(states.Receive, 0, states.Receive.Length, SocketFlags.None, EndReceive, states);
            }
            catch (Exception ex)
            {
                Close(ex, states.Socket);
            }
        }

        protected void EndReceive(IAsyncResult result)
        {
            SocketReceiveStates state = (SocketReceiveStates)result.AsyncState;
            int read;
            try
            {
                read = state.Socket.EndReceive(result);
                receiveBytes += read;
            }
            catch (Exception ex)
            {
                Close(ex, state.Socket);
                return;
            }
            if (read <= 0)
            {
                Close(null, state.Socket);
                return;
            }
            var bytes = new ArraySegment<byte>(state.Receive, 0, read);
            OnReceiveBytes(ref bytes);
            BeginReceive(state);
        }

        protected virtual void OnReceiveBytes(ref ArraySegment<byte> receive)
        {
            TriggerOnMessage(ref receive);
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

        protected virtual ProtocolType GetDefaultProtocolType()
        {
            return ProtocolType.Tcp;
        }

        protected virtual Socket MakeSocket()
        {
            return new Socket(LastIpEndPort.AddressFamily, SocketType.Stream, GetDefaultProtocolType());
        }
    }

}


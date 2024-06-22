using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace UniFan.Network
{
    public class NetChannel : INetChannel, IDisposable
    {
        protected enum NetworkEventTypes
        {
            Closed,
            Reconnecting,
            Reconnected,
            Packet,
            Error
        }

        protected struct NetworkEventMsg
        {
            public NetworkEventTypes DeliveryType;

            public object Context;
        }

        protected ConcurrentQueue<NetworkEventMsg> SyncNetworkEventQueue { get; private set; }

        private ISocket Socket { get; set; }

        public IMsgCodec MsgCodec { get; }

        public bool Connected => Socket.Connected;

        public int Ping
        {
            get
            {
                if (NetPing == null)
                {
                    return 0;
                }
                return NetPing.Ping;
            }
        }

        protected IReconnection Reconnection { get; private set; }

        protected IHeartBeat HeartBeat { get; private set; }

        protected IPing NetPing { get; private set; }

        private List<INetworkPlugin> plugins = new List<INetworkPlugin>();

        public string Name => Socket.Name;

        private volatile bool beConnected;

        private volatile bool reconnecting;

        protected bool BeConnected
        {
            get { return beConnected; }
            set { beConnected = value; }
        }

        protected bool Reconnecting
        {
            get { return reconnecting; }
            set { reconnecting = value; }
        }

        public long SentBytes => Socket.SentBytes;

        public long ReceiveBytes => Socket.ReceiveBytes;


        public event Action<INetChannel, Exception> OnClosed;

        public event Action<INetChannel, Exception> OnReconnecting;

        public event Action<INetChannel> OnReconnected;

        public event Action<INetChannel, object> OnPacket;

        public event Action<INetChannel, Exception> OnError;

        public NetChannel(ISocket socket, IMsgCodec codec)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket", "Param [socket] cannot be null");
            }
            if (codec == null)
            {
                throw new ArgumentNullException("codec", "Param [codec] cannot be null");
            }
            Socket = socket;
            MsgCodec = codec;
            //sendState = new SendState(Codec);
            SyncNetworkEventQueue = new ConcurrentQueue<NetworkEventMsg>();
            Reconnecting = false;
            BeConnected = false;
            Socket.OnMessage += OnChannelMessage;
            Socket.OnClosed += OnChannelClosed;
            Socket.OnError += OnChannelError;
        }

        protected virtual void Reset()
        {
            if (HeartBeat != null)
                HeartBeat.Reset();
            if (MsgCodec != null)
                MsgCodec.Reset();
        }

        public void Dispose()
        {
            Disconnect();
            Socket?.Dispose();
            Reset();
            Socket.OnMessage -= OnChannelMessage;
            Socket.OnClosed -= OnChannelClosed;
            Socket.OnError -= OnChannelError;
            this.SyncNetworkLoop();
        }

        public virtual INetChannel SetPlugins(INetworkPlugin plugs)
        {
            if (plugs is IReconnection)
            {
                SetReconnection((IReconnection)plugs);
            }
            if (plugs is IHeartBeat)
            {
                SetHeartBeat((IHeartBeat)plugs);
            }
            if (plugs is IPing)
            {
                SetPing((IPing)plugs);
            }
            if (plugs != null)
            {
                plugs.SetNetChannel(this);
            }
            return this;
        }

        private void SetReconnection(IReconnection reconnection)
        {
            Reconnection = reconnection;
            plugins.Add(reconnection);
        }

        private void SetHeartBeat(IHeartBeat heartBeat)
        {
            HeartBeat = heartBeat;
            plugins.Add(heartBeat);
        }

        private void SetPing(IPing netPing)
        {
            NetPing = netPing;
            plugins.Add(netPing);
        }

        public Task<SocketConnectResult> Connect()
        {
            Reset();
            return Socket.Connect();
        }

        public Task<SocketConnectResult> Connect(IPEndPoint ipEndPoint)
        {
            Reset();
            Socket.ChangeIpEndPoint(ipEndPoint);
            return ConnectInternal();
        }

        public Task<SocketConnectResult> Connect(Uri uri)
        {
            Reset();
            Socket.ChangeUri(uri);
            return ConnectInternal();
        }

        protected async Task<SocketConnectResult> ConnectInternal()
        {
            var result = await Socket.Connect();

            if (result.Result == ConnectResults.Success)
            {
                BeConnected = true;
                if (Reconnecting)
                {
                    try
                    {
                        Reconnection.Reconnected();
                    }
                    finally
                    {
                        Reconnecting = false;
                        EnqueueNetworkEvent(NetworkEventTypes.Reconnected);
                    }
                }

            }
            return result;
        }

        public SendResults Send(object packet)
        {
            try
            {
                ReadOnlySpan<byte> result = MsgCodec.Pack(packet);
                return Send(result);
            }
            catch (Exception ex)
            {
                EnqueueNetworkEvent(NetworkEventTypes.Error, ex);
                return SendResults.Faild;
            }
        }

        public SendResults Send(byte[] source)
        {
            if (!Socket.Connected)
            {
                return SendResults.Faild;
            }
            if (source == null)
            {
                return SendResults.Ignore;
            }
            return Send(source, 0, source.Length);
        }


        public SendResults Send(byte[] source, int offset, int count)
        {
            if (!Socket.Connected)
            {
                return SendResults.Faild;
            }
            if (source == null || count <= 0)
            {
                return SendResults.Ignore;
            }
            return Socket.Send(source, offset, count);
        }

        public SendResults Send(ReadOnlySpan<byte> source)
        {
            if (!Socket.Connected)
            {
                return SendResults.Faild;
            }
            if (source.Length <= 0)
            {
                return SendResults.Ignore;
            }
            return Socket.Send(source);
        }

        protected virtual void OnChannelClosed(ISocket socket, Exception ex)
        {
            Reconnecting = false;
            EnqueueNetworkEvent(NetworkEventTypes.Closed, ex);
        }


        protected virtual void OnChannelError(ISocket socket, Exception ex)
        {
            EnqueueNetworkEvent(NetworkEventTypes.Error, ex);
        }

        protected virtual void OnChannelMessage(ISocket socket, ArraySegment<byte> source)
        {
            try
            {
                Exception ex;
                int count = source.Count;
                while (MsgCodec.Input(source.Array, source.Offset, count, out var receive, out ex))
                {
                    if (HeartBeat != null)
                    {
                        HeartBeat.Reset();
                    }

                    object packet = MsgCodec.Unpack(receive);
                    try
                    {
                        if (NetPing != null)
                        {
                            NetPing.AcceptPacket(packet);
                        }
                    }
                    catch (Exception ex2)
                    {
                        Disconnect(ex2);
                        return;
                    }
                    EnqueueNetworkEvent(NetworkEventTypes.Packet, packet);
                    count = 0;
                }
                if (ex != null)
                {
                    Disconnect(ex);
                }
            }
            catch (Exception ex)
            {
                EnqueueNetworkEvent(NetworkEventTypes.Error, ex);
            }
        }

        protected void TriggerError(Exception exception)
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

        public CloseResults Disconnect(Exception exception = null)
        {
            return Socket.Disconnect(exception);
        }


        public virtual bool Reconnect(Exception ex)
        {
            if (Reconnection == null || !BeConnected)
            {
                Reconnecting = false;
                return false;
            }
            if (Reconnecting = Reconnection.Reconnect(ex))
            {
                EnqueueNetworkEvent(NetworkEventTypes.Reconnecting, ex);
            }
            return Reconnecting;
        }


        public virtual void OnUpdate(float deltaTime, float unsacaleTime)
        {
            if (Socket != null)
            {
                Socket.OnUpdate(deltaTime, unsacaleTime);
                if (Socket.Connected)
                {
                    for (int i = 0; i < plugins.Count; i++)
                    {
                        plugins[i].OnUpdate(deltaTime, unsacaleTime);
                    }
                }
            }

            SyncNetworkLoop();
        }

        private void SyncNetworkLoop()
        {

            while (SyncNetworkEventQueue.TryDequeue(out var data))
            {
                try
                {

                    OnNetworkEvent(data);
                }
                catch (Exception ex)
                {
                    EnqueueNetworkEvent(NetworkEventTypes.Error, ex);
                }
            }

        }

        protected virtual void EnqueueNetworkEvent(NetworkEventTypes networkType, object context = null)
        {
            var eventMsg = new NetworkEventMsg
            {
                DeliveryType = networkType,
                Context = context
            };
            EnqueueNetworkEvent(networkType, ref eventMsg);
        }

        protected virtual void EnqueueNetworkEvent(NetworkEventTypes networkType, ref NetworkEventMsg netEvent)
        {
            SyncNetworkEventQueue.Enqueue(netEvent);
        }

        protected virtual void OnNetworkEvent(NetworkEventMsg delivery)
        {
            switch (delivery.DeliveryType)
            {
                case NetworkEventTypes.Packet:
                    DeliveryPacket(delivery.Context);
                    break;
                case NetworkEventTypes.Closed:
                    DeliveryClosed(delivery.Context as Exception);
                    break;
                case NetworkEventTypes.Reconnecting:
                    DeliveryReconnecting(delivery.Context as Exception);
                    break;
                case NetworkEventTypes.Reconnected:
                    DeliveryReconnected();
                    break;
                case NetworkEventTypes.Error:
                    DeliveryError(delivery.Context as Exception);
                    break;
                default:
                    throw new Exception("Unknow delivery type [" + delivery.DeliveryType + "]");
            }
        }


        protected virtual void DeliveryPacket(object packet)
        {
            if (this.OnPacket != null)
            {
                this.OnPacket(this, packet);
            }
        }

        protected virtual void DeliveryConnectingInline(Action action)
        {
            action?.Invoke();
        }

        protected virtual void DeliveryClosed(Exception ex)
        {
            if (this.OnClosed != null)
            {
                this.OnClosed(this, ex);
            }
        }

        protected virtual void DeliveryReconnecting(Exception ex)
        {
            if (this.OnReconnecting != null)
            {
                this.OnReconnecting(this, ex);
            }
        }

        protected virtual void DeliveryReconnected()
        {
            if (this.OnReconnected != null)
            {
                this.OnReconnected(this);
            }
        }

        protected virtual void DeliveryError(Exception ex)
        {
            if (this.OnError != null)
            {
                try
                {
                    this.OnError(this, ex);
                }
                catch (Exception)
                {
                }
            }
        }
    }

}

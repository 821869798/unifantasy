using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace UniFan.Network
{
    public class NetChannel : INetChannel, IDisposable
    {
        protected enum NetworkEventTypes
        {
            ConnectingInline,
            Connecting,
            Connected,
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

        public event Action<INetChannel, IPEndPoint> OnConnecting;

        public event Action<INetChannel> OnConnected;

        public event Action<INetChannel, Exception> OnClosed;

        public event Action<INetChannel, Exception> OnReconnecting;

        public event Action<INetChannel> OnReconnected;

        public event Action<INetChannel, IMsgPacket> OnPacket;

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
            Socket.OnConnecting += OnChannelConnecting;
            Socket.OnConnected += OnChannelConnected;
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
            Socket.OnConnecting -= OnChannelConnecting;
            Socket.OnConnected -= OnChannelConnected;
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

        public void Connect(Action<ConnectResults, Exception> callback = null)
        {
            Reset();
            Socket.Connect(MakeConnectInlineAction(callback));
        }

        public void Connect(IPEndPoint ipEndPoint, Action<ConnectResults, Exception> callback = null)
        {
            Reset();
            Socket.ChangeIpEndPoint(ipEndPoint);
            Socket.Connect(MakeConnectInlineAction(callback));
        }

        private Action<ConnectResults, Exception> MakeConnectInlineAction(Action<ConnectResults, Exception> callback)
        {
            if (callback == null)
            {
                return null;
            }
            return delegate (ConnectResults result, Exception ex)
            {
                Action context = delegate
                {
                    callback(result, ex);
                };
                EnqueueNetworkEvent(NetworkEventTypes.ConnectingInline, context);
            };
        }

        public SendResults Send(IMsgPacket packet)
        {
            try
            {
                ArraySegment<byte> result = MsgCodec.Pack(packet);
                return Send(result.Array, result.Offset, result.Count);
            }
            catch (Exception ex)
            {
                EnqueueNetworkEvent(NetworkEventTypes.Error, ex);
                return SendResults.Faild;
            }
        }

        public SendResults Send(byte[] source)
        {
            if (source == null)
            {
                return SendResults.Ignore;
            }
            if (!Socket.Connected)
            {
                return SendResults.Faild;
            }
            if (source != null)
            {
                return Send(source, 0, source.Length);
            }
            return SendResults.Success;
        }

        public SendResults Send(byte[] source, int offset)
        {
            if (source == null)
            {
                return SendResults.Ignore;
            }
            if (!Socket.Connected)
            {
                return SendResults.Faild;
            }
            if (source != null)
            {
                return Send(source, offset, source.Length - offset);
            }
            return SendResults.Success;
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


        protected virtual void OnChannelConnecting(ISocket channel)
        {
            if (!Connected)
            {
                EnqueueNetworkEvent(NetworkEventTypes.Connecting);
            }
        }

        protected virtual void OnChannelConnected(ISocket channel)
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
                return;
            }

            EnqueueNetworkEvent(NetworkEventTypes.Connected);
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

                    IMsgPacket packet = MsgCodec.Unpack(receive);
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
                    DeliveryPacket(delivery.Context as IMsgPacket);
                    break;
                case NetworkEventTypes.ConnectingInline:
                    DeliveryConnectingInline(delivery.Context as Action);
                    break;
                case NetworkEventTypes.Connecting:
                    DeliveryConnecting(delivery.Context as IPEndPoint);
                    break;
                case NetworkEventTypes.Connected:
                    DeliveryConnected();
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


        protected virtual void DeliveryPacket(IMsgPacket packet)
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

        protected virtual void DeliveryConnecting(IPEndPoint ipEndPoint)
        {
            if (this.OnConnecting != null)
            {
                this.OnConnecting(this, ipEndPoint);
            }
        }

        protected virtual void DeliveryConnected()
        {
            if (this.OnConnected != null)
            {
                this.OnConnected(this);
            }
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

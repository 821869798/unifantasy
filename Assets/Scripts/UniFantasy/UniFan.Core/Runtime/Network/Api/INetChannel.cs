using System;
using System.Net;
using System.Threading.Tasks;

namespace UniFan.Network
{
    public interface INetChannel
    {
        string Name { get; }

        bool Connected { get; }

        int Ping { get; }

        long SentBytes { get; }

        long ReceiveBytes { get; }

        IMsgCodec MsgCodec { get; }


        event Action<INetChannel, Exception> OnClosed;

        event Action<INetChannel, Exception> OnReconnecting;

        event Action<INetChannel> OnReconnected;

        event Action<INetChannel, object> OnPacket;

        event Action<INetChannel, Exception> OnError;

        SendResults Send(object packet);

        SendResults Send(byte[] source);

        SendResults Send(byte[] source, int offset, int count);

        SendResults Send(ReadOnlySpan<byte> source);

        Task<SocketConnectResult> Connect();

        Task<SocketConnectResult> Connect(IPEndPoint ipEndPoint);

        Task<SocketConnectResult> Connect(Uri uri);

        bool Reconnect(Exception ex);

        CloseResults Disconnect(Exception ex = null);

        INetChannel SetPlugins(INetworkPlugin plugs);

        void OnUpdate(float deltaTime, float unscaleTime);

    }
}

using System;
using System.Net;

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

        event Action<INetChannel, IPEndPoint> OnConnecting;

        event Action<INetChannel> OnConnected;

        event Action<INetChannel, Exception> OnClosed;

        event Action<INetChannel, Exception> OnReconnecting;

        event Action<INetChannel> OnReconnected;

        event Action<INetChannel, IMsgPacket> OnPacket;

        event Action<INetChannel, Exception> OnError;

        SendResults Send(IMsgPacket packet);

        SendResults Send(byte[] source);

        SendResults Send(byte[] source, int offset, int count);
        
        SendResults Send(ReadOnlySpan<byte> source);

        void Connect(Action<ConnectResults, Exception> callback = null);

        void Connect(IPEndPoint ipEndPoint, Action<ConnectResults, Exception> callback = null);

        void Connect(Uri uri, Action<ConnectResults, Exception> callback = null);
        
        bool Reconnect(Exception ex);

        CloseResults Disconnect(Exception ex = null);

        INetChannel SetPlugins(INetworkPlugin plugs);

        void OnUpdate(float deltaTime, float unscaleTime);

    }
}

using System;
using System.Net;

namespace UniFan.Network
{
    public interface ISocket : IDisposable
    {
        string Name
        {
            get;
        }

        bool Connected
        {
            get;
        }

        long SentBytes
        {
            get;
        }

        long ReceiveBytes
        {
            get;
        }

        event Action<ISocket> OnConnecting;

        event Action<ISocket> OnConnected;

        event Action<ISocket, Exception> OnClosed;

        event Action<ISocket, ArraySegment<byte>> OnMessage;

        event Action<ISocket, Exception> OnError;

        void ChangeIpEndPoint(IPEndPoint ipEndPoint);

        void Connect(Action<ConnectResults, Exception> callback = null);

        SendResults Send(byte[] data);

        SendResults Send(byte[] data, int offset);

        SendResults Send(byte[] data, int offset, int count);

        CloseResults Disconnect(Exception exception = null);

        void OnUpdate(float deltaTime, float unsacaleTime);
    }


}

using System;
using System.Net;
using System.Threading.Tasks;

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

        event Action<ISocket, Exception> OnClosed;

        event Action<ISocket, ArraySegment<byte>> OnMessage;

        event Action<ISocket, Exception> OnError;

        void ChangeIpEndPoint(IPEndPoint ipEndPoint);

        void ChangeUri(Uri uri);

        Task<SocketConnectResult> Connect();

        SendResults Send(byte[] data);

        SendResults Send(byte[] data, int offset, int count);

        SendResults Send(ReadOnlySpan<byte> data);

        CloseResults Disconnect(Exception exception = null);

        void OnUpdate(float deltaTime, float unsacaleTime);
    }

    public struct SocketConnectResult
    {
        public ConnectResults Result { internal set; get; }
        public Exception Exception { internal set; get; }
    }

}

using System;
using System.Net;


namespace UniFan.Network
{
    public interface ISocketFactory
    {
        ISocket Create(string protocol, IPEndPoint ipEndPoint = null);

        void Extend(string protocol, Func<IPEndPoint, ISocket> maker);

    }
}

using System;
using System.Collections.Generic;
using System.Net;

namespace UniFan.Network
{
    public class SocketFactory : SingleFactory<SocketFactory>, ISocketFactory
    {

        private readonly Dictionary<string, Func<IPEndPoint, ISocket>> socketMaker = new Dictionary<string, Func<IPEndPoint, ISocket>>();

        private readonly object syncRoot = new object();

        protected override void Initialize()
        {
            this.Extend("frame.tcp", (IPEndPoint ipEndPoint) => new TcpConnector("frame.tcp", ipEndPoint));
            this.Extend("kcp", (IPEndPoint ipEndPoint) => new KcpConnector("kcp", ipEndPoint));
        }

        public ISocket Create(string name, IPEndPoint ipEndPoint = null)
        {
            lock (syncRoot)
            {
                Guard.NotEmptyOrNull(name, "socket name");
                if (!socketMaker.TryGetValue(name, out Func<IPEndPoint, ISocket> maker))
                {
                    throw new RuntimeException("Undefined socket protocol [" + name + "]");
                }
                ISocket socket = maker(ipEndPoint);
                return socket;
            }
        }

        private string NormalProtocol(string protocol)
        {
            return protocol.ToLower();
        }

        public void Extend(string protocol, Func<IPEndPoint, ISocket> maker)
        {
            lock (syncRoot)
            {
                Guard.NotEmptyOrNull(protocol, "protocol");
                Guard.Requires<ArgumentNullException>(maker != null);
                socketMaker.Add(NormalProtocol(protocol), maker);
            }
        }

    }
}

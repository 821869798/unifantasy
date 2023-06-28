using System;
using System.Collections.Generic;
using System.Net;

namespace UniFan.Network
{
    public class SocketFactory : SingleFactory<SocketFactory>, ISocketFactory
    {

        private readonly Dictionary<string, Func<object, ISocket>> socketMaker = new Dictionary<string, Func<object, ISocket>>();

        private readonly object syncRoot = new object();

        protected override void Initialize()
        {
            this.Extend("tcp", (object ipOrUri) => new TcpConnector("tcp", ipOrUri as IPEndPoint));
            this.Extend("kcp", (object ipOrUri) => new KcpConnector("kcp", ipOrUri as IPEndPoint));
            this.Extend("ws", (object ipOrUri) => new WSConnector("ws", ipOrUri as Uri));
        }

        public ISocket Create(string name, IPEndPoint ipEndPoint = null)
        {
            return CreateInternal(name, ipEndPoint);
        }

        public ISocket Create(string name, Uri uri = null)
        {
            return CreateInternal(name, uri);
        }

        private ISocket CreateInternal(string name, object ipOrUri = null)
        {
            lock (syncRoot)
            {
                Guard.NotEmptyOrNull(name, "socket name");
                if (!socketMaker.TryGetValue(name, out Func<object, ISocket> maker))
                {
                    throw new RuntimeException("Undefined socket protocol [" + name + "]");
                }
                ISocket socket = maker(ipOrUri);
                return socket;
            }
        }

        private string NormalProtocol(string protocol)
        {
            return protocol.ToLower();
        }

        public void Extend(string protocol, Func<object, ISocket> maker)
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

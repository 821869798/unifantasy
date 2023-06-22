using System;

namespace UniFan.Network
{
    public interface IPing : INetworkPlugin
    {
        int Ping { get; }

        void AcceptPacket(object packet);
    }
}

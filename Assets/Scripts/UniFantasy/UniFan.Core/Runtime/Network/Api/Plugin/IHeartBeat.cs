using System;

namespace UniFan.Network
{
    public interface IHeartBeat : INetworkPlugin
    {

        void Reset();

        bool MissHeartBeat(int count);
    }

}

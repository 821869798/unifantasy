using System;

namespace UniFan.Network
{
    public interface IReconnection : INetworkPlugin
    {
        bool Reconnect(Exception lastException);

        void Reconnected();
    }


}

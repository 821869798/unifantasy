using System;


namespace UniFan.Network
{
    public interface INetworkPlugin
    {
        void OnUpdate(float deltaTime, float unscaleTime);

        void SetNetChannel(INetChannel netChannel);
    }

}

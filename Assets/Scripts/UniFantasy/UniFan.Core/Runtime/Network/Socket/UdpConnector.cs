using System;
using System.Net;
using System.Net.Sockets;

namespace UniFan.Network
{
    internal class UdpConnector : SocketConnector
    {
        public float CfgReceiveTimeout { protected set; get; }

        protected float receiveTimeout;
        protected bool receiving;

        public UdpConnector(string name, IPEndPoint ipEndPoint, float receiveTimeout)
            : base(name, ipEndPoint, ProtocolType.Udp)
        {
            this.CfgReceiveTimeout = receiveTimeout;
        }

        protected override void BeginReceive(SocketReceiveStates states)
        {
            try
            {
                states.Socket.BeginReceive(states.Receive, 0, states.Receive.Length, SocketFlags.None, EndReceive, states);
                receiveTimeout = timestamp + CfgReceiveTimeout;
                receiving = true;
            }
            catch (Exception ex)
            {
                Close(ex, states.Socket);
            }
        }

        public override void OnUpdate(float deltaTime, float unsacaleTime)
        {
            base.OnUpdate(deltaTime, unsacaleTime);
            if (receiving && receiveTimeout <= timestamp)
            {
                Close(new TimeoutException("Socket receive timeout"), Socket);
            }
        }

        protected override void OnReset()
        {
            base.OnReset();
            receiving = false;
        }

        protected override System.Net.Sockets.Socket MakeSocket()
        {
            return new System.Net.Sockets.Socket(base.LastIpEndPort.AddressFamily, SocketType.Dgram, base.ProtocolType);
        }
    }


}

using System;

namespace UniFan.Network
{
    public interface IFragment
    {
        int MaxPackageSize
        {
            get;
        }

        int Input(ArraySegment<byte> source, out Exception ex);

        ArraySegment<byte> Receive(ArraySegment<byte> source, out Exception ex);

        ArraySegment<byte> Send(ArraySegment<byte> source, out Exception ex);
    }

}

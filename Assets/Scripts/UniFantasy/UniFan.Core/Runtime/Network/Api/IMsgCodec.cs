using System;

namespace UniFan.Network
{
    public interface IMsgCodec
    {
        int MaxPackageSize { get; }

        bool Input(byte[] source, int offset, int count, out ArraySegment<byte> result, out Exception ex);

        void Reset();

        ArraySegment<byte> Pack(object packet);

        object Unpack(ArraySegment<byte> packet);
    }
}

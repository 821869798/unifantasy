using System;

namespace UniFan.Network
{
    public interface IMsgCodec
    {
        bool Input(byte[] source, int offset, int count, out ArraySegment<byte> result, out Exception ex);

        void Reset();

        ArraySegment<byte> Pack(IMsgPacket packet);

        IMsgPacket Unpack(ArraySegment<byte> rawData);

        IMsgPacket CreatePacket();
    }
}

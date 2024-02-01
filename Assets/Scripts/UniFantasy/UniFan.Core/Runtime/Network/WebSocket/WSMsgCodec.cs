
using System;

namespace UniFan.Network
{
    public class WSMsgCodec : IMsgCodec
    {
        public WSMsgCodec()
        {
        }

        public virtual bool Input(byte[] source, int offset, int count, out ReadOnlySpan<byte> result, out Exception ex)
        {
            ex = null;
            result = default;
            try
            {
                result = new ReadOnlySpan<byte>(source, offset, count);
                return true;
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }
            return false;
        }


        public virtual void Reset()
        {

        }

        public virtual IMsgPacket CreatePacket()
        {
            return WSMsgPacket.Get();
        }

        public virtual ReadOnlySpan<byte> Pack(IMsgPacket packet)
        {
            return packet.Output();
        }

        public virtual IMsgPacket Unpack(ReadOnlySpan<byte> rawData)
        {
            var packet = CreatePacket();
            packet.Input(rawData);
            return packet;
        }
    }
}

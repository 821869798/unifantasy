
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
            if (count <= 0)
            {
                return false;
            }
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

        public virtual ReadOnlySpan<byte> Pack(object packet)
        {
            if (packet is WSMsgPacket msgPacket)
            {
                return msgPacket.OutputSpan();
            }
            throw new ArgumentException("packet is not WSMsgPacket");
        }

        public virtual object Unpack(ReadOnlySpan<byte> rawData)
        {
            var packet = WSMsgPacket.Get();
            packet.Input(rawData);
            return packet;
        }
    }
}

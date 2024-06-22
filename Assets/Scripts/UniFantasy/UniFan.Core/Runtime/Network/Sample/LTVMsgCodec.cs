using System;

namespace UniFan.Network
{
    public class LTVMsgCodec : DefaultMsgCodec
    {
        public LTVMsgCodec() : base(false)
        {
        }

        public override ReadOnlySpan<byte> Pack(object packet)
        {
            if (packet is LTVMsgPacket ltvMsgPacket)
            {
                return ltvMsgPacket.OutputSpan();
            }
            throw new ArgumentException("packet is not LTVMsgPacket");
        }

        public override object Unpack(ReadOnlySpan<byte> rawData)
        {
            var packet = LTVMsgPacket.Get();
            packet.Input(rawData);
            return packet;
        }
    }
}

using System;

namespace UniFan.Network
{
    public class LTVMsgCodec : DefaultMsgCodec
    {
        public LTVMsgCodec() : base(false)
        {
        }

        public override ArraySegment<byte> Pack(object packet)
        {
            if (packet is LTVMsgData msgData)
            {
                return msgData.Output();
            }
            return new ArraySegment<byte>();
        }

        public override object Unpack(ArraySegment<byte> packet)
        {
            LTVMsgData msgData = LTVMsgData.Get();
            msgData.Input(packet);
            return msgData;
        }
    }
}

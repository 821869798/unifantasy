using System;

namespace UniFan.Network
{
    public class LTVMsgCodec : DefaultMsgCodec
    {
        public LTVMsgCodec() : base(false)
        {
        }

        public override IMsgPacket CreatePacket()
        {
            return LTVMsgPacket.Get();
        }
    }
}

using System;
using System.Collections.Generic;

namespace UniFan.Network
{
    public partial class LTVMsgPacket : BaseMsgPacket<LTVMsgPacket>
    {

        public ByteArray ByteData { private set; get; } = new ByteArray(BigEndianOrder.Instance);

        public const int TotalPackHeadLen = 8;

        public override void Input(ArraySegment<byte> packet)
        {
            this.ByteData.WriteBytes(packet.Array, packet.Offset, packet.Count);
            var msgLen = this.ByteData.ReadUInt32();
            CmdId = this.ByteData.ReadUInt32();
        }

        public override ArraySegment<byte> Output()
        {
            return ByteData.GetRawBytes();
        }

        public override void Encode()
        {
            this.ByteData.WriteUInt32((uint)TotalPackHeadLen).WriteUInt32(this.CmdId);
        }

        public override void Encode(byte[] data, int offset, int len)
        {
            int msgLen = len;
            this.ByteData.WriteUInt32((uint)(msgLen + TotalPackHeadLen)).WriteUInt32(this.CmdId).WriteBytes(data, offset, len);
        }

        public override void Reset()
        {
            CmdId = 0;
            ByteData.Clear();
        }

        public override bool CanReturnPool()
        {
            return this.ByteData.Capacity <= 1024;
        }

    }
}

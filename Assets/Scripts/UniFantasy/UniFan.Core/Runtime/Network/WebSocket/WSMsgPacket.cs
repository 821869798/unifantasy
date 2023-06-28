using System;
using System.Collections.Generic;

namespace UniFan.Network
{
    public class WSMsgPacket : BaseMsgPacket<WSMsgPacket>
    {

        public ByteArray ByteData { private set; get; } = new ByteArray(BigEndianOrder.Instance);


        public override void Input(ArraySegment<byte> packet)
        {
            this.ByteData.WriteBytes(packet.Array, packet.Offset, packet.Count);
            CmdId = this.ByteData.ReadUInt32();
        }

        public override ArraySegment<byte> Output()
        {
            return ByteData.GetRawBytes();
        }

        public override void Encode()
        {
            this.ByteData.WriteUInt32(this.CmdId);
        }

        public override void Encode(byte[] data, int offset, int len)
        {
            this.ByteData.WriteUInt32(this.CmdId).WriteBytes(data, offset, len);
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

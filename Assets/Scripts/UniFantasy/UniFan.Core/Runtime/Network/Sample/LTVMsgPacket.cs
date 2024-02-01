using System;
using System.Collections.Generic;

namespace UniFan.Network
{
    public partial class LTVMsgPacket : BaseMsgPacket<LTVMsgPacket>
    {
        public ByteArray ByteData { private set; get; }

        public const int TotalPackHeadLen = 8;

        public LTVMsgPacket()
        {
            CreateByteArray();
        }

        public virtual void CreateByteArray()
        {
            ByteData = new ByteArray(BigEndianOrder.Instance);
        }

        public override void Input(ReadOnlySpan<byte> packet)
        {
            this.ByteData.WriteSpan(packet);
            var msgLen = this.ByteData.ReadUInt32();
            CmdId = this.ByteData.ReadUInt32();
        }

        public override ArraySegment<byte> Output()
        {
            return ByteData.GetRawBytes();
        }

        public override ReadOnlySpan<byte> OutputSpan()
        {
            return ByteData.GetReadOnlySpan();
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
        
        public override void Encode(ReadOnlySpan<byte> data)
        {
            int msgLen = data.Length;
            this.ByteData.WriteUInt32((uint)(msgLen + TotalPackHeadLen)).WriteUInt32(this.CmdId).WriteSpan(data);
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

using System;

namespace UniFan.Network
{
    public class WSMsgPacket : BaseMsgPacket<WSMsgPacket>
    {

        public ByteArray ByteData { private set; get; }

        public object UserData { private set; get; }

        public WSMsgPacket()
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
            this.ByteData.WriteUInt32(this.CmdId);
        }

        public override void Encode(byte[] data, int offset, int len)
        {
            this.ByteData.WriteUInt32(this.CmdId).WriteBytes(data, offset, len);
        }

        public override void Encode(ReadOnlySpan<byte> data)
        {
            this.ByteData.WriteUInt32(this.CmdId).WriteSpan(data);
        }

        public override void Reset()
        {
            CmdId = 0;
            ByteData.Clear();
            UserData = null;
        }

        public override bool CanReturnPool()
        {
            return this.ByteData.Capacity <= 1024;
        }
    }
}

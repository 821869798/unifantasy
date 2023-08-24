using System;

namespace UniFan.Network
{
    /// <summary>
    /// 默认的消息编解码器，使用头部4个字节来做包头大小判定
    /// </summary>
    public abstract class DefaultMsgCodec : IMsgCodec
    {
        public virtual int MaxPackageSize => 131072;  //128k

        public const int headerLength = 4;

        protected int packetLength;

        protected readonly ByteArray byteArray;

        protected IByteOrder byteOrder;

        protected DefaultMsgCodec(bool isLittleEndian)
        {
            byteOrder = isLittleEndian ? LittleEndianOrder.Instance : BigEndianOrder.Instance;
            byteArray = new ByteArray(isLittleEndian, MaxPackageSize);
        }

        public virtual bool Input(byte[] source, int offset, int count, out ArraySegment<byte> result, out Exception ex)
        {
            ex = null;
            result = default;
            try
            {
                if (count > 0)
                {
#if UNITY_EDITOR
                    //编辑器下检测最大空间，防止因为bug引起的包太大。真机不需要，byteArray可以自动扩容
                    if (byteArray.availableCapacity < count)
                    {
                        throw new Exception("The received packet has exceeded the maximum limit:" + byteArray.Capacity);
                    }
#endif
                    byteArray.WriteBytes(source, offset, count);
                }
                if (byteArray.ValidCount <= 0)
                {
                    return false;
                }
                if (packetLength <= 0)
                {
                    packetLength = GetPacketLength(byteArray.GetRawBytes());
                    if (packetLength <= 0)
                    {
                        return false;
                    }
                }
                if (packetLength > byteArray.ValidCount)
                {
                    return false;
                }
                var bytes = this.byteArray.GetRawBytes();
                result = new ArraySegment<byte>(bytes.Array, bytes.Offset, packetLength);
                this.byteArray.SkipReaderCount(packetLength);
                packetLength = 0;
                return true;
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }
            return false;
        }

        protected virtual int GetPacketLength(ArraySegment<byte> source)
        {
            if (source.Count < headerLength)
            {
                return 0;
            }
            int packetLen = byteOrder.ToInt32(source.Array, source.Offset);
            return packetLen;
        }

        public virtual void Reset()
        {
            byteArray.Clear();
            packetLength = 0;
        }

        public abstract IMsgPacket CreatePacket();

        public virtual ArraySegment<byte> Pack(IMsgPacket packet)
        {
            return packet.Output();
        }

        public virtual IMsgPacket Unpack(ArraySegment<byte> rawData)
        {
            var packet = CreatePacket();
            packet.Input(rawData);
            return packet;
        }

    }
}

using System;
using System.Collections.Generic;

namespace UniFan.Network
{
    public partial class LTVMsgData
    {
        //消息ID
        public uint CmdId { get; set; }

        public ByteArray ByteData { private set; get; } = new ByteArray(BigEndianOrder.Instance);

        public const int TotalPackHeadLen = 8;

        public void Input(ArraySegment<byte> packet)
        {
            this.ByteData.WriteBytes(packet.Array, packet.Offset, packet.Count);
            var msgLen = this.ByteData.ReadUInt32();
            CmdId = this.ByteData.ReadUInt32();
        }

        public void Encode()
        {
            this.ByteData.WriteUInt32((uint)TotalPackHeadLen).WriteUInt32(this.CmdId);
        }

        public void Encode(byte[] data, int offset, int len)
        {
            int msgLen = len;
            this.ByteData.WriteUInt32((uint)(msgLen + TotalPackHeadLen)).WriteUInt32(this.CmdId).WriteBytes(data, offset, len);
        }

        public void Encode(byte[] data)
        {
            Encode(data, 0, data.Length);
        }

        public void Encode(ArraySegment<byte> data)
        {
            Encode(data.Array, data.Offset, data.Count);
        }

        //public void Encode(IMessage message)
        //{
        //    Encode(message.ToByteArray());
        //}

        //public T Decode<T>() where T : class, pb::IMessage, new()
        //{
        //    T msg = new T();
        //    msg.MergeFrom(this.RawData, 0, this.RawDataLen);
        //    return msg;
        //}

        public ArraySegment<byte> Output()
        {
            return ByteData.GetRawBytes();
        }

        public void Reset()
        {
            CmdId = 0;
            ByteData.Clear();
        }

        #region Object Pool

        private static readonly Stack<LTVMsgData> thisObjPool = new Stack<LTVMsgData>();
        private static object syncObject = new object();
        private const int MAX_OJBECT_POOL = 20;

        public static LTVMsgData Get()
        {
            lock (syncObject)
            {
                if (thisObjPool.Count > 0)
                {
                    return thisObjPool.Pop();
                }
                return new LTVMsgData();
            }
        }

        public void Put()
        {
            if (this.ByteData.Capacity > 1024)
                return;
            lock (syncObject)
            {
                this.Reset();
                if (thisObjPool.Count < MAX_OJBECT_POOL)
                {
                    thisObjPool.Push(this);
                }
            }
        }

        #endregion
    }
}

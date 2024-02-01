using System;
using System.Collections.Generic;

namespace UniFan.Network
{
    /// <summary>
    /// 消息包
    /// </summary>
    public interface IMsgPacket
    {
        void Input(ReadOnlySpan<byte> rawData);
        ArraySegment<byte> Output();
        ReadOnlySpan<byte> OutputSpan();
        void Encode();
        void Encode(byte[] data, int offset, int len);
        void Encode(byte[] data);
        void Encode(ReadOnlySpan<byte> data);
        bool CanReturnPool();
        void Reset();
        void Put();
    }



    public abstract class BaseMsgPacket<T> : IMsgPacket where T : BaseMsgPacket<T>, new()
    {
        /// <summary>
        /// 带消息id的基础消息，也可以完全不用，自己定义
        /// </summary>
        public virtual uint CmdId { get; set; }

        public abstract void Input(ReadOnlySpan<byte> rawData);

        public abstract ArraySegment<byte> Output();
        public abstract ReadOnlySpan<byte> OutputSpan();

        public void Encode(byte[] data)
        {
            Encode(data, 0, data.Length);
        }

        public abstract void Encode(ReadOnlySpan<byte> data);
        public abstract void Encode();

        public abstract void Encode(byte[] data, int offset, int len);

        public abstract bool CanReturnPool();

        public abstract void Reset();

        #region ObjectPool
        private static readonly Stack<T> objPool = new Stack<T>();
        private static readonly object syncObject = new object();
        private const int MAX_OBJ_POOL = 20;

        public void Put()
        {
            if (!this.CanReturnPool())
            {
                return;
            }
            lock (syncObject)
            {
                this.Reset();
                if (objPool.Count < MAX_OBJ_POOL)
                {
                    objPool.Push((T)this);
                }
            }
        }

        public static T Get()
        {
            lock (syncObject)
            {
                if (objPool.Count > 0)
                {
                    return objPool.Pop();
                }
                return new T();
            }
        }
        #endregion;

    }
}

using System;


namespace UniFan.Network
{
    public interface IByteOrder
    {
        bool IsLittleEndian { get; }

        public byte[] GetBytes(bool value);

        public byte[] GetBytes(short value);

        public byte[] GetBytes(int value);

        public byte[] GetBytes(long value);

        public byte[] GetBytes(ushort value);

        public byte[] GetBytes(uint value);

        public byte[] GetBytes(ulong value);

        public byte[] GetBytes(float value);

        public byte[] GetBytes(double value);

        public void PutToBytes(bool value, byte[] bytes, int startIndex);

        public void PutToBytes(short value, byte[] bytes, int startIndex);

        public void PutToBytes(int value, byte[] bytes, int startIndex);

        public void PutToBytes(long value, byte[] bytes, int startIndex);

        public void PutToBytes(ushort value, byte[] bytes, int startIndex);

        public void PutToBytes(uint value, byte[] bytes, int startIndex);

        public void PutToBytes(ulong value, byte[] bytes, int startIndex);

        public void PutToBytes(float value, byte[] bytes, int startIndex);

        public void PutToBytes(double value, byte[] bytes, int startIndex);

        public bool ToBoolean(byte[] value, int startIndex);

        public short ToInt16(byte[] value, int startIndex);

        public int ToInt32(byte[] value, int startIndex);

        public long ToInt64(byte[] value, int startIndex);

        public ushort ToUInt16(byte[] value, int startIndex);

        public uint ToUInt32(byte[] value, int startIndex);

        public ulong ToUInt64(byte[] value, int startIndex);

        public float ToFloat(byte[] value, int startIndex);

        public double ToDouble(byte[] value, int startIndex);

    }
}

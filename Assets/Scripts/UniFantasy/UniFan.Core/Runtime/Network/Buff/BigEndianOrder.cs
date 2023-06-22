using System;

namespace UniFan.Network
{
    public class BigEndianOrder : IByteOrder
    {
        public bool IsLittleEndian => false;

        private BigEndianOrder() { }

        public static BigEndianOrder Instance { get; } = new BigEndianOrder();

        public byte[] GetBytes(bool value)
        {
            return new byte[1] { (byte)(value ? 1 : 0) };
        }

        public byte[] GetBytes(short value)
        {
            return GetBytes((ushort)value);
        }

        public byte[] GetBytes(int value)
        {
            return GetBytes((uint)value);
        }

        public byte[] GetBytes(long value)
        {
            return GetBytes((ulong)value);
        }

        public byte[] GetBytes(ushort value)
        {
            byte[] array = new byte[2];
            array[0] = (byte)(value >> 8);
            array[1] = (byte)value;
            return array;
        }

        public byte[] GetBytes(uint value)
        {
            byte[] array = new byte[4];
            array[0] = (byte)(value >> 24);
            array[1] = (byte)(value >> 16);
            array[2] = (byte)(value >> 8);
            array[3] = (byte)value;
            return array;
        }

        public byte[] GetBytes(ulong value)
        {
            byte[] array = new byte[8];
            array[0] = (byte)(value >> 56);
            array[1] = (byte)(value >> 48);
            array[2] = (byte)(value >> 40);
            array[3] = (byte)(value >> 32);
            array[4] = (byte)(value >> 24);
            array[5] = (byte)(value >> 16);
            array[6] = (byte)(value >> 8);
            array[7] = (byte)value;
            return array;
        }

        public byte[] GetBytes(float value)
        {
            int intValue = BitConverter.SingleToInt32Bits(value);
            return GetBytes((uint)intValue);
        }

        public byte[] GetBytes(double value)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);
            return GetBytes((ulong)longValue);
        }

        public void PutToBytes(bool value, byte[] bytes, int startIndex)
        {
            bytes[startIndex] = (byte)(value ? 1 : 0);
        }

        public void PutToBytes(short value, byte[] bytes, int startIndex)
        {
            PutToBytes((ushort)value, bytes, startIndex);
        }

        public unsafe void PutToBytes(int value, byte[] bytes, int startIndex)
        {
            PutToBytes((uint)value, bytes, startIndex);
        }

        public unsafe void PutToBytes(long value, byte[] bytes, int startIndex)
        {
            PutToBytes((long)value, bytes, startIndex);
        }

        public unsafe void PutToBytes(ushort value, byte[] bytes, int startIndex)
        {
            fixed (byte* ptr = &bytes[startIndex])
            {
                *ptr = (byte)(value >> 8);
                ptr[1] = (byte)value;
            }
        }

        public unsafe void PutToBytes(uint value, byte[] bytes, int startIndex)
        {
            fixed (byte* ptr = &bytes[startIndex])
            {
                *ptr = (byte)(value >> 24);
                ptr[1] = (byte)(value >> 16);
                ptr[2] = (byte)(value >> 8);
                ptr[3] = (byte)value;
            }
        }

        public unsafe void PutToBytes(ulong value, byte[] bytes, int startIndex)
        {
            fixed (byte* ptr = &bytes[startIndex])
            {
                *ptr = (byte)(value >> 56);
                ptr[1] = (byte)(value >> 48);
                ptr[2] = (byte)(value >> 40);
                ptr[3] = (byte)(value >> 32);
                ptr[4] = (byte)(value >> 24);
                ptr[5] = (byte)(value >> 16);
                ptr[6] = (byte)(value >> 8);
                ptr[7] = (byte)value;
            }
        }

        public unsafe void PutToBytes(float value, byte[] bytes, int startIndex)
        {
            int intValue = BitConverter.SingleToInt32Bits(value);
            PutToBytes((uint)intValue, bytes, startIndex);
        }

        public unsafe void PutToBytes(double value, byte[] bytes, int startIndex)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);
            PutToBytes((ulong)longValue, bytes, startIndex);
        }

        public bool ToBoolean(byte[] value, int startIndex)
        {
            if (value[startIndex] != 0)
            {
                return true;
            }
            return false;
        }

        public unsafe double ToDouble(byte[] value, int startIndex)
        {
            long num = ToInt64(value, startIndex);
            return *(double*)(&num);
        }

        public unsafe float ToFloat(byte[] value, int startIndex)
        {
            int num = ToInt32(value, startIndex);
            return *(float*)(&num);
        }

        public unsafe short ToInt16(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                return (short)((*ptr << 8) | ptr[1]);
            }
        }

        public unsafe int ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                return (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
            }
        }

        public unsafe long ToInt64(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                int num3 = (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
                int num4 = (ptr[4] << 24) | (ptr[5] << 16) | (ptr[6] << 8) | ptr[7];
                return (uint)num4 | ((long)num3 << 32);
            }
        }

        public ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)ToInt16(value, startIndex);
        }

        public uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)ToInt32(value, startIndex);
        }

        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong)ToInt64(value, startIndex);
        }


    }
}

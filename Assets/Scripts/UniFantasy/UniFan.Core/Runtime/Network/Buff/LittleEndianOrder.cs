using System;

namespace UniFan.Network
{
    public class LittleEndianOrder : IByteOrder
    {
        public bool IsLittleEndian => true;

        private LittleEndianOrder() { }

        public static LittleEndianOrder Instance { get; } = new LittleEndianOrder();

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
            array[0] = (byte)value;
            array[1] = (byte)(value >> 8);
            return array;
        }

        public byte[] GetBytes(uint value)
        {
            byte[] array = new byte[4];
            array[0] = (byte)value;
            array[1] = (byte)(value >> 8);
            array[2] = (byte)(value >> 16);
            array[3] = (byte)(value >> 24);
            return array;
        }

        public byte[] GetBytes(ulong value)
        {
            byte[] array = new byte[8];
            array[0] = (byte)value;
            array[1] = (byte)(value >> 8);
            array[2] = (byte)(value >> 16);
            array[3] = (byte)(value >> 24);
            array[4] = (byte)(value >> 32);
            array[5] = (byte)(value >> 40);
            array[6] = (byte)(value >> 48);
            array[7] = (byte)(value >> 56);

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
                *ptr = (byte)value;
                ptr[1] = (byte)(value >> 8);
            }
        }

        public unsafe void PutToBytes(uint value, byte[] bytes, int startIndex)
        {
            fixed (byte* ptr = &bytes[startIndex])
            {
                *ptr = (byte)value;
                ptr[1] = (byte)(value >> 8);
                ptr[2] = (byte)(value >> 16);
                ptr[3] = (byte)(value >> 24);
            }
        }

        public unsafe void PutToBytes(ulong value, byte[] bytes, int startIndex)
        {
            fixed (byte* ptr = &bytes[startIndex])
            {
                *ptr = (byte)value;
                ptr[1] = (byte)(value >> 8);
                ptr[2] = (byte)(value >> 16);
                ptr[3] = (byte)(value >> 24);
                ptr[4] = (byte)(value >> 32);
                ptr[5] = (byte)(value >> 40);
                ptr[6] = (byte)(value >> 48);
                ptr[7] = (byte)(value >> 56);
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
                return (short)(*ptr | (ptr[1] << 8));
            }
        }

        public unsafe int ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                return *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
            }
        }

        public unsafe long ToInt64(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                int num = *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
                int num2 = ptr[4] | (ptr[5] << 8) | (ptr[6] << 16) | (ptr[7] << 24);
                return (uint)num | ((long)num2 << 32);
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

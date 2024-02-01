using System;
using System.Text;
using UniFan.Network;

public class ByteArray
{
    public IByteOrder byteOrder { get; set; }

    public int Capacity { private set; get; }

    /// <summary>
    /// 可用的空间，包括通过腾挪的
    /// </summary>
    public int availableCapacity => availableWriteLen + readIndex;

    public int ValidCount => writeIndex - readIndex;

    //字节缓存区
    private byte[] rawdata;

    //读取索引
    private int readIndex = 0;
    //写入索引
    private int writeIndex = 0;

    // 可写的长度
    private int availableWriteLen => Capacity - writeIndex;

    //默认空间
    public const int DEFAULT_CAP = 256;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ByteArray(bool isLittleEndian, int c = DEFAULT_CAP)
    {
        Capacity = c;
        rawdata = new byte[Capacity];
        this.byteOrder = isLittleEndian ? LittleEndianOrder.Instance : BigEndianOrder.Instance;
    }

    /// <summary>
    /// 构造指定大小的buffer
    /// </summary>
    /// <param name="len">Array容量</param>
    public ByteArray(IByteOrder order, int c = DEFAULT_CAP)
    {
        Capacity = c;
        rawdata = new byte[Capacity];
        this.byteOrder = order;
    }


    public ByteArray Clear()
    {
        readIndex = 0;
        writeIndex = 0;
        return this;
    }

    /// <summary>
    /// 获取原始的字节数组，不会有GC
    /// 方便拿去做反序列化
    /// </summary>
    /// <returns></returns>
    public ArraySegment<byte> GetRawBytes()
    {
        var segment = new ArraySegment<byte>(rawdata, readIndex, ValidCount);
        return segment;
    }

    public ReadOnlySpan<byte> GetReadOnlySpan()
    {
        return new ReadOnlySpan<byte>(rawdata, readIndex, ValidCount);
    }

    /// <summary>
    /// 跳过一定数量的读取
    /// </summary>
    public void SkipReaderCount(int len)
    {
        readIndex = Math.Min(readIndex + len, writeIndex);
    }

    public ByteArray Reverse()
    {
        if (ValidCount <= 0)
        {
            return this;
        }
        return this.Reverse(readIndex, ValidCount);
    }

    private ByteArray Reverse(int offset, int len)
    {
        Array.Reverse(rawdata, offset, len);
        return this;
    }

    public ByteArray WriteByte(byte value)
    {
        CheckAndExpand(1);
        rawdata[writeIndex++] = value;
        return this;
    }

    public ByteArray WriteBoolean(bool value)
    {
        WriteByte(value ? (byte)1 : (byte)0);
        return this;
    }

    public ByteArray WriteInt16(short value)
    {
        CheckAndExpand(2);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 2;
        return this;
    }

    public ByteArray WriteUInt16(ushort value)
    {
        CheckAndExpand(2);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 2;
        return this;
    }

    public ByteArray WriteInt32(int value)
    {
        CheckAndExpand(4);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 4;
        return this;
    }

    public ByteArray WriteUInt32(uint value)
    {
        CheckAndExpand(4);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 4;
        return this;
    }

    public ByteArray WriteInt64(long value)
    {
        CheckAndExpand(8);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 8;
        return this;
    }

    public ByteArray WriteUInt64(ulong value)
    {
        CheckAndExpand(8);
        byteOrder.PutToBytes(value, rawdata, writeIndex);
        writeIndex += 8;
        return this;
    }

    public ByteArray WriteSpan(ReadOnlySpan<byte> value)
    {
        CheckAndExpand(value.Length);
        Span<byte> destinationSpan = rawdata.AsSpan().Slice(writeIndex);
        value.CopyTo(destinationSpan);
        writeIndex += value.Length;
        return this;
    }
    
    public ByteArray WriteBytes(byte[] value)
    {
        return WriteBytes(value, 0, value.Length);
    }

    public ByteArray WriteBytes(byte[] value, int offset, int len)
    {
        CheckAndExpand(len);
        
        Buffer.BlockCopy(value, offset, rawdata, writeIndex, len);
        writeIndex += len;

        return this;
    }

    public void WriteUTFBytes(string value)
    {
        byte[] temp = Encoding.UTF8.GetBytes(value);
        WriteBytes(temp);
    }

    private void CheckAndExpand(int len)
    {
        while (!CanWrite(len))
        {
            ExpandCapacity(len);
        }
    }

    private bool CanWrite(int len)
    {
        if (availableWriteLen < len)
        {
            return false;
        }
        return true;
    }

    private void ExpandCapacity(int len)
    {
        int validCount = ValidCount;
        if (readIndex + availableWriteLen >= len)
        {
            //可以腾挪之前的位置
            if (validCount > 0)
            {
                Buffer.BlockCopy(rawdata, readIndex, rawdata, 0, validCount);
            }
            readIndex = 0;
            writeIndex = ValidCount;
            return;
        }

        byte[] oldData = rawdata;

        do
        {
            Capacity = Capacity <= 0 ? DEFAULT_CAP : Capacity * 2;

        } while (Capacity < len + validCount);

        rawdata = new byte[Capacity];
        if (validCount > 0)
        {
            Buffer.BlockCopy(oldData, readIndex, rawdata, 0, validCount);
        }
        readIndex = 0;
        writeIndex = ValidCount;

    }

    public byte ReadByte()
    {
        if (ValidCount < 1)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        byte result = rawdata[readIndex++];
        return result;
    }

    public byte[] ReadBytes(int len)
    {
        if (ValidCount < len)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        byte[] result = new byte[len];
        Buffer.BlockCopy(rawdata, readIndex, result, 0, len);
        readIndex += len;
        return result;
    }

    public short ReadInt16()
    {
        if (ValidCount < 2)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        short result = byteOrder.ToInt16(rawdata, readIndex);
        readIndex += 2;
        return result;
    }

    public ushort ReadUInt16()
    {
        if (ValidCount < 2)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        ushort result = byteOrder.ToUInt16(rawdata, readIndex);
        readIndex += 2;
        return result;
    }

    public int ReadInt32()
    {
        if (ValidCount < 4)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        int result = byteOrder.ToInt32(rawdata, readIndex);
        readIndex += 4;
        return result;
    }

    public uint ReadUInt32()
    {
        if (ValidCount < 4)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        uint result = byteOrder.ToUInt32(rawdata, readIndex);
        readIndex += 4;
        return result;
    }

    public long ReadInt64()
    {
        if (ValidCount < 8)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        long result = byteOrder.ToInt64(rawdata, readIndex);
        readIndex += 8;
        return result;
    }

    public ulong ReadUInt64()
    {
        if (ValidCount < 8)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        ulong result = byteOrder.ToUInt64(rawdata, readIndex);
        readIndex += 8;
        return result;
    }

    public string ReadUTFByte(int len)
    {
        if (ValidCount < len)
        {
            throw new InvalidOperationException("Not enough data to read");
        }
        string result = Encoding.UTF8.GetString(rawdata, readIndex, len);
        readIndex += len;
        return result;
    }

}

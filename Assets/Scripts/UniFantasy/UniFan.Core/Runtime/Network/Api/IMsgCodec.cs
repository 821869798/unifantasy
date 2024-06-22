using System;

namespace UniFan.Network
{
    /// <summary>
    /// 消息编解码器
    /// </summary>
    public interface IMsgCodec
    {
        bool Input(byte[] source, int offset, int count, out ReadOnlySpan<byte> result, out Exception ex);

        void Reset();

        ReadOnlySpan<byte> Pack(object packet);

        object Unpack(ReadOnlySpan<byte> rawData);
    }
}

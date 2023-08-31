using System;

namespace UniFan
{
    /// <summary>
    /// 使用Id索引的map，但是保证内部的遍历的固定性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BetterLinearMap<T> : BetterLinearMapBase<T> where T : class
    {
        public BetterLinearMap(int cap = 16) : base(cap)
        {
        }

        /// <summary>
        /// 添加值
        /// </summary>
        /// <param name="value"></param>
        /// <returns>返回一个id</returns>
        public uint Add(uint id, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return AddInternal(id, value);
        }
    }
}
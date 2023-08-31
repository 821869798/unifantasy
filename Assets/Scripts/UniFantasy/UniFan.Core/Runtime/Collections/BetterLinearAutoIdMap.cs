using System;

namespace UniFan
{
    /// <summary>
    /// 使用Id索引的map，但是保证内部的遍历的固定性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BetterLinearAutoIdMap<T> : BetterLinearMapBase<T> where T : class
    {
        protected uint _idGenerator = 0;

        public BetterLinearAutoIdMap(int cap = 16) : base(cap)
        {
        }

        public override void Clear()
        {
            base.Clear();
            _idGenerator = 0;
        }

        /// <summary>
        /// 生成新id
        /// </summary>
        /// <returns></returns>
        protected virtual uint GenerateId()
        {
            do
            {
                _idGenerator++;
            }
            while (_idGenerator == 0 || _indexMaping.ContainsKey(_idGenerator));
            return _idGenerator;
        }

        /// <summary>
        /// 添加值
        /// </summary>
        /// <param name="value"></param>
        /// <returns>返回一个id</returns>
        public uint Add(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            var id = GenerateId();
            return AddInternal(id, value);
        }

    }
}
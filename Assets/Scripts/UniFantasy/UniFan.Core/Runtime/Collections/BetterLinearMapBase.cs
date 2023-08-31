using System;
using System.Collections;
using System.Collections.Generic;

namespace UniFan
{
    /// <summary>
    /// 使用Id索引的map，但是保证内部的遍历的固定性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BetterLinearMapBase<T> : IEnumerable<T>, IReadOnlyCollection<T> where T : class
    {
        protected int _version = 0;
        protected T[] _arrayData;
        protected Dictionary<uint, int> _indexMaping;

        protected int _lastRemoveIndex;

        public int Capacity { protected set; get; }

        public int IndexCount { protected set; get; }

        public int Count => _indexMaping.Count;

        public BetterLinearMapBase(int cap)
        {
            this.Capacity = cap;
            this._arrayData = new T[cap];
            this._lastRemoveIndex = -1;
            this._indexMaping = new Dictionary<uint, int>(cap);
        }

        /// <summary>
        /// 是否包含这个id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainId(uint id)
        {
            return _indexMaping.ContainsKey(id);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetValue(uint id)
        {
            if (_indexMaping.TryGetValue(id, out var index))
            {
                return _arrayData[index];
            }
            return null;
        }

        /// <summary>
        /// 直接通过数组下标获取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetValueByIndex(int index)
        {
            return _arrayData[index];
        }

        /// <summary>
        /// 添加值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected uint AddInternal(uint id, T value)
        {
            _version++;
            if (_lastRemoveIndex >= 0)
            {
                _arrayData[_lastRemoveIndex] = value;
                _indexMaping[id] = _lastRemoveIndex;
                _lastRemoveIndex = -1;
            }
            else
            {
                if (IndexCount > Count)
                {
                    //从前往后找到空位
                    for (int i = 0; i < IndexCount; i++)
                    {
                        if (_arrayData[i] == null)
                        {
                            _arrayData[i] = value;
                            _indexMaping[id] = i;
                            return id;
                        }
                    }
                }
                else if (IndexCount >= Capacity)
                {
                    ExpandCapacity();
                }

                _arrayData[IndexCount] = value;
                _indexMaping[id] = IndexCount;
                IndexCount++;
            }
            return id;
        }

        /// <summary>
        /// 通过id删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Remove(uint id)
        {
            if (_indexMaping.TryGetValue(id, out var index))
            {
                _version++;
                if (index == IndexCount - 1 && IndexCount > Count)
                {
                    int lastIndex = index;
                    while (lastIndex > 0 && _arrayData[lastIndex - 1] == null)
                    {
                        lastIndex--;
                        IndexCount--;
                    }
                    _lastRemoveIndex = lastIndex;
                }
                else
                {
                    _lastRemoveIndex = index;
                }
                var value = _arrayData[index];
                _arrayData[index] = null;
                _indexMaping.Remove(id);
                return value;
            }
            return null;
        }

        /// <summary>
        /// 清除所有
        /// </summary>
        public virtual void Clear()
        {
            if (_indexMaping.Count > 0 || IndexCount > 0)
            {

                _indexMaping.Clear();
                IndexCount = 0;
                _lastRemoveIndex = -1;
                _version++;
            }
        }

        /// <summary>
        /// 扩容数组
        /// </summary>
        /// <param name="len"></param>
        protected void ExpandCapacity()
        {
            var newCap = Capacity << 1;
            var newArray = new T[newCap];
            _arrayData.CopyTo(newArray, 0);
            _arrayData = newArray;
            Capacity = newCap;
        }


        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #region GetEnumerator

        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly T[] _array;
            private readonly int _count;
            private int _index;

            private readonly int _version;
            private readonly BetterLinearMapBase<T> _parent;

            private T current;

            public T Current => current;

            object IEnumerator.Current => Current;

            internal Enumerator(BetterLinearMapBase<T> parent)
            {
                _array = parent._arrayData;
                _count = parent.IndexCount;
                _version = parent._version;
                _parent = parent;
                _index = -1;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_parent._version != _version)
                {
                    throw new InvalidOperationException("BetterLinearIdMap Collection was modified; enumeration operation may not execute.");
                }

                while (++_index < _count)
                {
                    var value = _array[_index];
                    if (value != null)
                    {
                        current = value;
                        return true;
                    }
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
                current = default(T);
            }
        }
        #endregion
    }
}
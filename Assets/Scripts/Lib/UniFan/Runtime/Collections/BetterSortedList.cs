using System.Collections;
using System.Collections.Generic;
using System;


namespace UniFan
{
    public class BetterSortedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private List<KeyValuePair<TKey, TValue>> m_sortedList;
        private Dictionary<TKey, TValue> m_internalDic;

        private IComparer<TKey> keyComparer;
        private Comparison<KeyValuePair<TKey, TValue>> sortComparer;
        public int Count => m_sortedList.Count;

        /// <summary>
        /// 是否忽略排序的标志位
        /// </summary>
        public bool IgnoreSort { get; private set; }

        public BetterSortedList(int cap, bool ignoreSort = false)
        {
            m_sortedList = new List<KeyValuePair<TKey, TValue>>(cap);
            m_internalDic = new Dictionary<TKey, TValue>(cap);

            InternalInit(ignoreSort);
        }

        public BetterSortedList(bool ignoreSort = false)
        {
            m_sortedList = new List<KeyValuePair<TKey, TValue>>();
            m_internalDic = new Dictionary<TKey, TValue>();
            InternalInit(ignoreSort);
        }

        private void InternalInit(bool ignoreSort)
        {
            IgnoreSort = ignoreSort;
            keyComparer = Comparer<TKey>.Default;
            if (!ignoreSort)
            {
                sortComparer = (v1, v2) => keyComparer.Compare(v1.Key, v2.Key);
            }
        }

        public KeyValuePair<TKey, TValue> GetByIndex(int index)
        {
            return m_sortedList[index];
        }

        public void RemoveByIndex(int index)
        {
            var v = m_sortedList[index];
            m_internalDic.Remove(v.Key);
            m_sortedList.RemoveAt(index);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_internalDic.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return m_internalDic.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            m_sortedList.Add(new KeyValuePair<TKey, TValue>(key, value));
            if (!IgnoreSort)
            {
                m_sortedList.Sort(sortComparer);
            }
            m_internalDic.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            var success = m_internalDic.Remove(key);
            if (success)
            {
                for (int i = 0; i < m_sortedList.Count; i++)
                {
                    //相同
                    if (m_sortedList[i].Key.Equals(key))
                    {
                        m_sortedList.RemoveAt(i);
                        break;
                    }
                }
            }
            return success;
        }

        public void Clear()
        {
            m_sortedList.Clear();
            m_internalDic.Clear();
        }

        public List<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator()
        {
            return m_sortedList.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return m_sortedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_sortedList.GetEnumerator();
        }


        /// <summary>
        /// 倒序遍历 并获取符合判定条件的第一个value
        /// </summary>
        /// <param name="onForeach"></param>
        public TValue ForeachInverseAndGetTheFirstValue(Func<TKey, bool> onForeach)
        {
            if (onForeach == null)
            {
                return default;
            }

            for (int i = m_sortedList.Count - 1; i >= 0; i--)
            {
                if (onForeach.Invoke(m_sortedList[i].Key))
                {
                    return m_sortedList[i].Value;
                }
            }

            return default;
        }
    }
}


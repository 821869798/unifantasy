using System.Collections;
using System.Collections.Generic;

namespace UniFan
{
    public class BetterList<TKey, TValue> : IEnumerable<TValue>
    {
        private List<TValue> m_betterList;
        private Dictionary<TKey, TValue> m_internalDic;
        public int Count => m_betterList.Count;

        public BetterList(int cap)
        {
            m_betterList = new List<TValue>(cap);
            m_internalDic = new Dictionary<TKey, TValue>(cap);

        }

        public BetterList()
        {
            m_betterList = new List<TValue>();
            m_internalDic = new Dictionary<TKey, TValue>();
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
            m_betterList.Add(value);
            m_internalDic.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            var bSuccess = false;
            if (m_internalDic.TryGetValue(key, out TValue tValue))
            {
                bSuccess = m_internalDic.Remove(key);
                if (bSuccess)
                {
                    int count = m_betterList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        //相同
                        if (m_betterList[i].Equals(tValue))
                        {
                            if (i == count - 1)
                            {
                                m_betterList.RemoveAt(i);
                            }
                            else
                            {
                                m_betterList[i] = m_betterList[count - 1];
                                m_betterList.RemoveAt(count - 1);
                            }
                            break;
                        }
                    }
                }
            }

            return bSuccess;
        }

        public void Clear()
        {
            m_betterList.Clear();
            m_internalDic.Clear();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return m_betterList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_betterList.GetEnumerator();
        }
    }
}
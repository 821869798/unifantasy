using System;
using System.Collections.Generic;

namespace UniFan
{
    public abstract class TObject : IDisposable
    {
        List<TObject> m_autoDisposeList;
        List<TObject> AutoDisposeList => m_autoDisposeList ?? (m_autoDisposeList = ListPool<TObject>.Get());

        public event Action OnDisposeEvent;

        protected void AutoDispose(TObject obj)
        {
            if (!AutoDisposeList.Contains(obj))
                AutoDisposeList.Add(obj);
        }

        public virtual void OnUpdate(float deltaTime) { }

        protected virtual void OnDispose() { }

        public virtual void Dispose()
        {
            if (m_autoDisposeList != null)
            {
                for (int i = 0; i < m_autoDisposeList.Count; i++)
                    m_autoDisposeList[i]?.Dispose();
                ListPool<TObject>.Put(m_autoDisposeList);
                m_autoDisposeList = null;
            }

            if (OnDisposeEvent != null)
            {
                OnDisposeEvent();
                OnDisposeEvent = null;
            }

            OnDispose();
        }
    }
}

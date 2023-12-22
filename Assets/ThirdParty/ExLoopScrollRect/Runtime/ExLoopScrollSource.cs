
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [Serializable]
    public class ExLoopScrollSource : LoopScrollDataSource, LoopScrollPrefabSource, LoopScrollSizeHelper
    {
        [NonSerialized] public Action<GameObject> eventCreateItemObject;
        [NonSerialized] public Action<GameObject> eventReturnItemObject;
        [NonSerialized] public Action<GameObject, int> eventItemDataChange;
        [NonSerialized] public Func<int, Vector2> eventGetItemSize;

        [SerializeField]
        public GameObject prefab;

        bool inited;
        private Stack<GameObject> pool;
        private Transform poolHolder;

        public void Init(Transform rooTransform)
        {
            if (inited)
                return;

            inited = true;

            pool = new Stack<GameObject>();

            var holder = new GameObject("PoolHolder");
            holder.SetActive(false);
            poolHolder = holder.transform;
            poolHolder.SetParent(rooTransform, false);
            prefab.transform.SetParent(poolHolder);
        }

        public Vector2 GetItemsSize(int itemsCount)
        {
            return eventGetItemSize.Invoke(itemsCount);
        }

        public GameObject GetObject(int index)
        {
            if (pool.Count > 0)
            {
                var go = pool.Pop();
                return go;
            }
            else
            {
                var go = Object.Instantiate(prefab);
                eventCreateItemObject?.Invoke(go);
                return go;
            }
        }

        public void ProvideData(Transform transform, int idx)
        {
            eventItemDataChange?.Invoke(transform.gameObject, idx);
        }

        public void ReturnObject(Transform go)
        {
            eventReturnItemObject?.Invoke(go.gameObject);
            pool.Push(go.gameObject);
            go.SetParent(poolHolder);
        }
    }
}

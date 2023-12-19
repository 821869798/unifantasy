using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotCode.Framework
{
    public class UIItemPool<T> : IDisposable where T : UIBaseNode, new()
    {
        public GameObject prefab { get; private set; }

        public List<T> listItems { get; private set; }
        protected Stack<T> poolItems { get; set; }

        public UIItemPool(GameObject prefab, bool hidePrefab = true)
        {
            this.prefab = prefab;

            if (hidePrefab)
            {
                this.prefab.SetActive(false);
            }

            this.listItems = new List<T>();
            this.poolItems = new Stack<T>();
        }

        public void HideAll()
        {
            foreach (var item in listItems)
            {
                item.Hide();
                this.poolItems.Push(item);
            }
            listItems.Clear();
        }

        public void HideOne(T item)
        {
            if (listItems.Remove(item))
            {
                item.Hide();
                this.poolItems.Push(item);
            }
        }

        public T GetOne(bool isShow = true)
        {
            if (this.poolItems.Count > 0)
            {
                var item = this.poolItems.Pop();
                if (isShow)
                {
                    item.Show();
                }
                item.transform.SetAsLastSibling();
                this.listItems.Add(item);
                return item;
            }
            var go = GameObject.Instantiate(this.prefab, this.prefab.transform.parent);
            if (isShow)
            {
                go.SetActive(true);
            }
            var newitem = new T();
            newitem.Init(go);
            this.listItems.Add(newitem);
            return newitem;
        }

        public bool TryGetFirst(out T item)
        {
            if (this.listItems.Count > 0)
            {
                item = this.listItems[0];
                return true;
            }
            item = default(T);
            return false;
        }

        public void Dispose()
        {
            this.HideAll();
            while (this.poolItems.Count > 0)
            {
                this.poolItems.Pop().Dispose();
            }
        }
    }
}

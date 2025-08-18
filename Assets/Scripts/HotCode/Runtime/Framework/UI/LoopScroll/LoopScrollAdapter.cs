using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    /// <summary>
    /// 对循环列表更上层适配封装，让业务用起来简单
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoopScrollAdapter<T> where T : UIBaseNode, new()
    {
        public IExLoopScrollRect exLoopScrollRect { get; }

        public LoopScrollRectBase originLoopScrollRect { get; }

        public readonly Dictionary<GameObject, T> itemNodeMapping = new Dictionary<GameObject, T>();

        // 必选项，刷新复用的item用
        private Action<T, int> onItemNodeChanged;

        // 可选项，ScrollPosition位置变化时
        public LoopScrollRectBase.ScrollRectEvent onValueChanged { get => originLoopScrollRect.onValueChanged; set => originLoopScrollRect.onValueChanged = value; }

        // 可选项，一些特殊操作需要监听回池子，比如回池子的时候移除上面一个选中的GameObject。
        private Action<T> onItemReturnPool;

        public float horizontalNormalizedPosition => originLoopScrollRect.horizontalNormalizedPosition;

        public float verticalNormalizedPosition => originLoopScrollRect.verticalNormalizedPosition;

        public LoopScrollAdapter(IExLoopScrollRect loopScrollRect)
        {
            exLoopScrollRect = loopScrollRect;
            originLoopScrollRect = loopScrollRect.loopScrollRect;
            exLoopScrollRect.eventCreateItemObject = OnCreateItemObject;
            exLoopScrollRect.eventReturnItemObject = OnReturnItemObject;
            exLoopScrollRect.eventItemDataChange = OnItemDataChange;
        }

        public LoopScrollAdapter<T> BindItemNodeChanged(Action<T, int> onItemDataChanged)
        {
            this.onItemNodeChanged = onItemDataChanged;
            return this;
        }

        public LoopScrollAdapter<T> BindItemReturnPool(Action<T> onItemReturnPool)
        {
            this.onItemReturnPool = onItemReturnPool;
            return this;
        }

        public void RefillCells(int totalCount, int startItem = 0, float contentOffset = 0)
        {
            originLoopScrollRect.totalCount = totalCount;
            originLoopScrollRect.RefillCells(startItem, contentOffset);
        }

        public void RefillCellsFromEnd(int totalCount, int endItem = 0, float contentOffset = 0)
        {
            originLoopScrollRect.totalCount = totalCount;
            originLoopScrollRect.RefillCellsFromEnd(endItem, contentOffset);
        }

        public void RefreshCells()
        {
            originLoopScrollRect.RefreshCells();
        }

        public void ScrollToCell(int index, float speed)
        {
            originLoopScrollRect.ScrollToCell(index, speed);
        }

        public void ScrollToCellWithinTime(int index, float time)
        {
            originLoopScrollRect.ScrollToCellWithinTime(index, time);
        }

        public void StopMovement()
        {
            originLoopScrollRect.StopMovement();
        }

        public T GetItemByIndex(int index)
        {
            var go = exLoopScrollRect.GetCellByIndex(index);
            if (go == null)
            {
                return default(T);
            }
            if (itemNodeMapping.TryGetValue(go, out var item))
            {
                return item;
            }
            return default(T);
        }


        protected virtual void OnCreateItemObject(GameObject go)
        {
            var newitem = new T();
            newitem.Init(go);
            itemNodeMapping.Add(go, newitem);
        }

        protected virtual void OnReturnItemObject(GameObject go)
        {
            if (onItemReturnPool == null)
            {
                return;
            }
            if (itemNodeMapping.TryGetValue(go, out var item))
            {
                onItemReturnPool.Invoke(item);
            }
        }

        protected virtual void OnItemDataChange(GameObject go, int index)
        {
            if (onItemNodeChanged == null)
            {
                return;
            }
            if (itemNodeMapping.TryGetValue(go, out var item))
            {
                onItemNodeChanged.Invoke(item, index);
            }
        }
    }
}

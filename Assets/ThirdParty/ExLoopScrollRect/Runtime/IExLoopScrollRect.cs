using System;

namespace UnityEngine.UI
{
    public interface IExLoopScrollRect
    {
        public Action<GameObject> eventCreateItemObject { get; set; }
        public Action<GameObject> eventReturnItemObject { get; set; }
        public Action<GameObject, int> eventItemDataChange { get; set; }

        public Func<int, int, float> eventGetItemSize { get; set; }

        public ExLoopScrollSource source { get; }

        public LoopScrollRectBase loopScrollRect { get; }

        GameObject GetCellByIndex(int index);
    }
}

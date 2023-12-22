using System;

namespace UnityEngine.UI
{
    public interface IExLoopScrollRect
    {
        public Action<GameObject> eventCreateItemObject { get; set; }
        public Action<GameObject> eventReturnItemObject { get; set; }
        public Action<GameObject, int> eventItemDataChange { get; set; }

        public Func<int, Vector2> eventGetItemSize { get; set; }

        public ExLoopScrollSource source { get; }

        public LoopScrollRectBase loopScrollRect { get; }
    }
}

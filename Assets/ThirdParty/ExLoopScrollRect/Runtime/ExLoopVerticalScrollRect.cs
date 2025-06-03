using System;

namespace UnityEngine.UI
{
    public class ExLoopVerticalScrollRect : LoopVerticalScrollRect, IExLoopScrollRect
    {
        [SerializeField]
        private ExLoopScrollSource m_source;

        public Action<GameObject> eventCreateItemObject { get => m_source.eventCreateItemObject; set => m_source.eventCreateItemObject = value; }
        public Action<GameObject> eventReturnItemObject { get => m_source.eventReturnItemObject; set => m_source.eventReturnItemObject = value; }
        public Action<GameObject, int> eventItemDataChange { get => m_source.eventItemDataChange; set => m_source.eventItemDataChange = value; }

        public Func<int, Vector2> eventGetItemSize
        {
            get { return m_source.eventGetItemSize; }
            set
            {
                m_source.eventGetItemSize = value;
                if (value != null)
                {
                    this.sizeHelper = m_source;
                }
                else
                {
                    this.sizeHelper = null;
                }
            }
        }

        public ExLoopScrollSource source => m_source;

        public LoopScrollRectBase loopScrollRect => this;

        protected override void Awake()
        {
            base.Awake();
            this.prefabSource = m_source;
            this.dataSource = m_source;
            m_source.Init(transform);
        }


        public GameObject GetCellByIndex(int index)
        {
            if (totalCount >= 0 && (index < 0 || index >= totalCount))
            {
                Debug.LogWarningFormat("invalid index {0}", index);
                return null;
            }

            var start = reverseDirection ? itemTypeEnd : itemTypeStart;
            var end = reverseDirection ? itemTypeStart : itemTypeEnd;

            if (index >= start && index < end)
            {
                var child = content.GetChild(index - itemTypeStart);
                if (child != null)
                {
                    return child.gameObject;
                }
                else
                {
                    Debug.LogError($"index({index}) out of range({content.childCount})");
                    return null;
                }
            }

            return null;
        }
    }
}

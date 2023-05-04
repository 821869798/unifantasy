using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniFan
{
    /// <summary>
    /// UGUI事件监听类
    /// </summary>
    public class EventTriggerListener : EventTrigger
    {
        public delegate void BaseEventDelegate(GameObject go, BaseEventData eventData);
        public delegate void PointerEventDelegate(GameObject go, PointerEventData eventData);
        public event PointerEventDelegate onClick;
        public event PointerEventDelegate onDown;
        public event PointerEventDelegate onEnter;
        public event PointerEventDelegate onExit;
        public event PointerEventDelegate onUp;
        public event BaseEventDelegate onSelect;
        public event BaseEventDelegate onUpdateSelect;
        public event BaseEventDelegate onDeselect;

        public event PointerEventDelegate onBeginDrag;
        public event PointerEventDelegate onDrag;
        public event PointerEventDelegate onEndDrag;

        PointerEventData m_lastPointerData;

        public bool DeselectTriggerEndDrag { get; set; } = true;

        void Start()
        {
            var selectable = GetComponent<Selectable>();
            if (!selectable)
            {
                //有这个才可触发 select事件
                selectable = gameObject.AddComponent<Selectable>();
                selectable.transition = Selectable.Transition.None;
            }
        }

        static public EventTriggerListener Get(GameObject go)
        {
            if (go == null)
            {
                Debug.LogError("EventTriggerListener_go_is_NULL");
                return null;
            }
            else
            {
                EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
                if (listener == null) listener = go.AddComponent<EventTriggerListener>();
                return listener;
            }
        }

        static public EventTriggerListener Get(Component component)
        {
            if (component == null) return null;
            return Get(component.gameObject);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(gameObject, eventData);

            m_lastPointerData = eventData;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) onDrag(gameObject, eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag(gameObject, eventData);

            m_lastPointerData = null;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                if (onClick != null) onClick(gameObject, eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown(gameObject, eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null) onEnter(gameObject, eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null) onExit(gameObject, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null) onUp(gameObject, eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(gameObject, eventData);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(gameObject, eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (onDeselect != null) onDeselect(gameObject, eventData);

            //触发结束拖拽
            if (DeselectTriggerEndDrag && m_lastPointerData != null && m_lastPointerData.pointerDrag == gameObject)
                onEndDrag?.Invoke(gameObject, m_lastPointerData);
        }
    }

}

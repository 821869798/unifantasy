using System;
using UnityEngine;

namespace HotCode.Framework
{
    public abstract class UIBaseNode : IDisposable
    {
        public bool active { get; private set; }

        public GameObject gameObject { get; protected set; }

        public RectTransform transform { get; protected set; }

        public virtual void Init(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            this.transform = root.transform as RectTransform;
            this.gameObject = root;
            this.active = root.activeSelf;
            this.BeforeInit();
            this.OnInit();
            if (this.active)
            {
                this.OnShow();
            }
        }

        /// <summary>
        /// 一般用来绑定生成组件的代码
        /// </summary>
        protected virtual void BeforeInit()
        {

        }

        protected abstract void OnInit();

        public virtual void Dispose()
        {
            OnDispose();
        }

        public void Close()
        {
            UIManager.Instance.CloseWindow(this.GetType());
        }


        protected virtual void OnDispose()
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }

        public virtual void Show()
        {
            if (this.active)
            {
                return;
            }
            this.active = true;
            this.gameObject.SetActive(true);
            OnShow();
        }

        protected virtual void OnShow()
        {

        }

        public virtual void Hide()
        {
            if (!this.active)
            {
                return;
            }
            this.active = false;
            this.gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnHide()
        {

        }


    }

}

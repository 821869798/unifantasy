using UniFan;
using HotCode.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Game
{
    public class UINTestScrollItem : UIBaseNode
    {

        #region Template Generate,don't modify
        protected partial class UIB_UINTestScrollItem
        {
            #region ObjectBinding Generate 
            public UniFan.ExButton btn_ScrollItem { protected set; get; }
            public UnityEngine.UI.Text tex_Content { protected set; get; }
            protected virtual void InitBinding(ObjectBinding __binding)
            {
                __binding.TryGetVariableValue<UniFan.ExButton>("btn_ScrollItem", out var __tbv0);
                this.btn_ScrollItem = __tbv0;
                __binding.TryGetVariableValue<UnityEngine.UI.Text>("tex_Content", out var __tbv1);
                this.tex_Content = __tbv1;
            }

            #endregion ObjectBinding Generate 
        }
        #endregion Template Generate,don't modify

        /// <summary>
        /// 可以自定义修改的
        /// </summary>
        protected partial class UIB_UINTestScrollItem
        {
            public virtual void StartBinding(GameObject __go)
            {
                var binding = __go.GetComponent<ObjectBinding>();
                if (binding != null)
                {
                    this.InitBinding(binding);
                }
            }
        }
        protected UIB_UINTestScrollItem ui { get; set; }

        protected override void BeforeInit()
        {
            ui = new UIB_UINTestScrollItem();
            ui.StartBinding(gameObject);
        }

        protected override void OnInit()
        {

        }

        public void RefreshScrollItem(int idx)
        {
            this.ui.tex_Content.text = $"ScrollItem:{idx}";
        }


        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}
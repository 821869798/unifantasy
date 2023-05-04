using UniFan;
using HotCode.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace HotCode.FrameworkPlay
{
    public class UILogin : UIBaseWindow
    {
        /// <summary>
        /// 静态配置
        /// </summary>
        private static UICreateConfig _createConfig = new UICreateConfig()
        {
            prefabName = nameof(UILogin),
            parentPath = string.Empty,
            layer = EUILayer.Normal,
            permanent = false,
        };

        /// <summary>
        /// 创建UI的配置
        /// </summary>
        public override IUICreateConfig createConfig => _createConfig;

        #region Template Generate,don't modify
        protected partial class UIB_UILogin
        {
            #region ObjectBinding Generate 
            public UniFan.ExImage image { protected set; get; }
            public UniFan.ExButton button { protected set; get; }
            protected virtual void InitBinding(ObjectBinding __binding)
            {
                var __tbv0 = __binding.GetVariableByName("image");
                this.image = __tbv0.GetValue<UniFan.ExImage>();
                var __tbv1 = __binding.GetVariableByName("button");
                this.button = __tbv1.GetValue<UniFan.ExButton>();
            }

            #endregion ObjectBinding Generate 
        }
        #endregion Template Generate,don't modify

        /// <summary>
        /// 可以自定义修改的
        /// </summary>
        protected partial class UIB_UILogin
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
        protected UIB_UILogin ui { get; set; }

        protected override void BeforeInit()
        {
            ui = new UIB_UILogin();
            ui.StartBinding(gameObject);
        }

        protected override void OnInit()
        {
            this.ui.button.onClick.AddListener(BtnClicked);
        }

        private void BtnClicked()
        {
            Debug.Log("Button Clicked");
            this.ui.image.color = Color.blue;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}
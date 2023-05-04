using System;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    public class UIBackgroundAdapter : MonoBehaviour
    {
        public enum BGAdapter
        {
            Cull = 0,       //裁剪，缩放
            Stretch = 1,    //拉伸
            MovieCull = 2,  //视频裁剪适配
        }

        [SerializeField]
        bool m_adaptOnAwake = true;

        public BGAdapter adapterType;
        [Tooltip("在最终的大小上进行大小的偏移")]
        [SerializeField]
        Vector2 m_sizeOffset = Vector2.zero;
        [HideInInspector]
        [SerializeField]
        bool m_onlyWidth = false;

        void Awake()
        {
            if (m_adaptOnAwake)
                AdaptBgUI();
            //MsgDispatcher.AddListener(MsgEventType.OnScreenSizeChanged, OnScreenSizeChanged);
        }

        [ContextMenu(nameof(AdaptBgUI))]
        public void AdaptBgUI()
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                Vector2 size;
                switch (adapterType)
                {
                    case BGAdapter.Cull:
                        size = UIManager.Instance.BackgroundCullSize;
                        break;
                    case BGAdapter.Stretch:
                        size = rectTransform.sizeDelta;
                        if (m_onlyWidth)
                            size.x = UIManager.Instance.BackgroundStretchSize.x;
                        else
                            size = UIManager.Instance.BackgroundStretchSize;
                        break;
                    case BGAdapter.MovieCull:
                        size = UIManager.Instance.MovieCullSize;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                size += m_sizeOffset;
                rectTransform.sizeDelta = size;
            }
        }

        public void OnScreenSizeChanged()
        {
            AdaptBgUI();
        }


        private RenderTexture renderTexture;
        /// <summary>
        /// 适配背景模糊图
        /// </summary>
        /// <param name="rt"></param>
        public void SetBGBlurTexture(RenderTexture rt)
        {
            RawImage image = this.GetComponent<RawImage>();
            if (image != null)
            {
                image.texture = rt;
                this.renderTexture = rt;
            }
            else
            {
                UnityEngine.Object.Destroy(rt);
            }
        }
		
        private void OnDestroy()
        {
			if (renderTexture != null)
            {
                UnityEngine.Object.Destroy(renderTexture);
                renderTexture = null;
            }
            //MsgDispatcher.RemoveListener(MsgEventType.OnScreenSizeChanged, OnScreenSizeChanged);
        }
    }

}

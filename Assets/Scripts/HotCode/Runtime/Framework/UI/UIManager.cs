using System;
using System.Collections.Generic;
using UniFan;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotCode.Framework
{
    public partial class UIManager : ManagerSingleton<UIManager>
    {
        /// <summary>
        /// 根节点
        /// </summary>
        private Transform _uiRoot;
        public Transform UIRoot => _uiRoot;

        /// <summary>
        /// 根canvas
        /// </summary>
        private Canvas _rootCanvas;
        public Canvas RootCanvas => _rootCanvas;

        /// <summary>
        /// CanvasScaler全局唯一
        /// </summary>
        private CanvasScaler _uiCanvasScaler;
        public CanvasScaler UICanvasScaler => _uiCanvasScaler;
        private Camera _uiCamera;
        public Camera UICamera => _uiCamera;

        Camera _mainCam;
        public Camera MainCamera
        {
            get
            {
                if (_mainCam == null || _mainCam.gameObject.activeInHierarchy == false)
                {
                    _mainCam = Camera.main;
                }
                return _mainCam;
            }

        }

        public Vector2 BackgroundCullSize { get; private set; }

        public Vector2 MovieCullSize { get; private set; }
        public Vector2 BackgroundStretchSize { get; private set; }

        public float MatchWidthOrHeight { get; private set; }

        /// <summary>
        /// 持有所有UI界面的层级根
        /// </summary>

        private RectTransform[] _uiLayoutLevelRoot;
        private string[] _uiLayerName;


        #region 刘海异形屏适配
        //最大缺口高度
        public float MaxNotchValue { get; set; } = 6f;

        //当前缺口高度
        private float _curNotchValue = 0;
        public float CurNotchValue
        {
            get { return this._curNotchValue; }
            set
            {
                this._curNotchValue = Mathf.Clamp(value, 0, MaxNotchValue);
                Vector2 anchorMin = new Vector2(this._curNotchValue / 100, 0);
                Vector2 anchorMax = new Vector2(1 - this._curNotchValue / 100, 1);
                for (int i = 0; i < _uiLayoutLevelRoot.Length; i++)
                {
                    RectTransform transform = _uiLayoutLevelRoot[i];
                    if (transform != null)
                    {
                        transform.anchorMin = anchorMin;
                        transform.anchorMax = anchorMax;
                    }
                }
            }
        }
        #endregion

        #region Max Mask 最高全屏遮罩
        private UINoDrawRaycast _maxMask;
        public bool MaxMaskActive
        {
            get { return _maxMask.raycastTarget; }
            set { this._maxMask.raycastTarget = value; }
        }
        #endregion

        #region 分屏适配

        //允许屏幕尺寸改变
        public bool EnableScreenSizeChange { get; set; } = true;

        public override int managerPriority => 20;

        //用于编辑器模拟手机上分屏改变屏幕尺寸
        private int _lastWidth = 0;
        private int _lastHeight = 0;

        //反射调用Canvas更新
        private System.Reflection.MethodInfo _methodCanvasUpdate;
        #endregion

        protected override void InitManager()
        {
            //初始化反射
            Type typeCanvasScaler = typeof(CanvasScaler);
            _methodCanvasUpdate = typeCanvasScaler.GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //对象引用初始化
            _uiRoot = GameObject.Find("GameMain/UIRoot").transform;
            _uiCamera = GameObject.Find("GameMain/UICamera").GetComponent<Camera>();
            _rootCanvas = _uiRoot.GetComponent<Canvas>();
            _uiCanvasScaler = _uiRoot.GetComponent<CanvasScaler>();
            // 初始化层级
            var layerEnums = System.Enum.GetValues(typeof(EUILayer));
            _uiLayoutLevelRoot = new RectTransform[layerEnums.Length];
            _uiLayerName = new string[layerEnums.Length];
            _allLayerWindows = new List<UIBaseWindow>[layerEnums.Length];
            for (int i = 0; i < layerEnums.Length; i++)
            {
                var e = layerEnums.GetValue(i).ToString();

                RectTransform t = _uiRoot.GetChild(i).transform as RectTransform;

                UICoreHelper.ResetRootTransform(t);
                _uiLayoutLevelRoot[i] = t;
                _uiLayerName[i] = e;

                _allLayerWindows[i] = new List<UIBaseWindow>();
            }

            //屏幕尺寸
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            CurNotchValue = UIAdaptation.GetDefaultNotchValue();
            CalcScreenAdapter();

            InitMaxMask();
        }


        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            UpdateScreenSize();

            InvokeKeyEvent();
        }

        private void CalcScreenAdapter()
        {
            //UI适配
            Debug.Log($"Sreen size:{Screen.width} {Screen.height}");
            MatchWidthOrHeight = UIAdaptation.AdaptationCanvasScaler();
            Vector2 referenceResolution = UICanvasScaler.referenceResolution;
            AdaptationCanvasScaler(UICanvasScaler);
            BackgroundStretchSize = UIAdaptation.GenerateBackgroundStretchSize(referenceResolution);
            BackgroundCullSize = UIAdaptation.GenerateBackgroundCullSize(BackgroundStretchSize);
            MovieCullSize = UIAdaptation.GenerateMovieCullSize(BackgroundStretchSize);
        }

        //强制适配CanvasScaler，在一帧中完成
        public void AdaptationCanvasScaler(CanvasScaler canvasScaler, bool forceUpdate = false)
        {
            canvasScaler.matchWidthOrHeight = MatchWidthOrHeight;
            if (forceUpdate)
            {
                _methodCanvasUpdate?.Invoke(canvasScaler, null);
            }
        }

        //加载UI时的全局遮罩
        private void InitMaxMask()
        {
            _maxMask = _uiLayoutLevelRoot[(int)EUILayer.Max].Find<UINoDrawRaycast>("MaxMask");
            (_maxMask.transform as RectTransform).sizeDelta = BackgroundStretchSize;
            MaxMaskActive = false;
        }


        private void UpdateScreenSize()
        {
            if (!EnableScreenSizeChange)
            {
                return;
            }

            //判断手机屏幕尺寸变化的时候，需要刷新下
            int curWidth = Screen.width;
            int curHeight = Screen.height;
            if (curWidth != _lastWidth || curHeight != _lastHeight)
            {
                _lastWidth = curWidth;
                _lastHeight = curHeight;
                OnMutilWindowStateChanged();
            }
        }

        //当分屏的时候，窗口大小改变的时候
        public void OnMutilWindowStateChanged()
        {
            CalcScreenAdapter();
            //TODO 广播消息
            //MsgDispatcher.Broadcast(MsgEventType.OnScreenSizeChanged);
        }

        private void InvokeKeyEvent()
        {

            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
            {
                //响应返回键
                bool isClicked = false;
                for (int i = _allLayerWindows.Length - 1; i >= 0; i--)
                {
                    var layerWindows = _allLayerWindows[i];
                    for (int k = layerWindows.Count - 1; k >= 0; k--)
                    {
                        var window = layerWindows[k];
                        if (!window.active)
                        {
                            continue;
                        }
                        var btnArr = window.gameObject.GetComponentsInChildren<ExButton>();
                        foreach (var btn in btnArr)
                        {
                            if (btn.isBackButton)
                            {
                                ExecuteEvents.Execute<ISubmitHandler>(btn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                                isClicked = true;
                                break;
                            }
                        }
                    }
                    if (isClicked)
                    {
                        break;
                    }
                }
            }
        }

        #region public interface

        public Transform GetUILayoutLevelRoot(EUILayer layoutLevel)
        {
            return _uiLayoutLevelRoot[(int)layoutLevel];
        }

        public string GetUILayoutName(EUILayer layoutLevel)
        {
            return _uiLayerName[(int)layoutLevel];
        }

        public Vector2 World2UIPosition(Vector3 worldPosition, out bool inCamBack, RectTransform rect = null, Camera uiCam = null, Camera mainCam = null)
        {
            inCamBack = false;
            if (!mainCam)
                mainCam = MainCamera;
            if (!mainCam)
            {
                Debug.LogError("Main camera is none!");
                return Vector2.zero;
            }
            else
            {
                var sPos = mainCam.WorldToScreenPoint(worldPosition);
                //sPos /= RenderManager.GetUIRatio(mainCam);    //世界相机有将分辨率的情况
                var localPoint = Screen2UIPosition(sPos, rect, uiCam);
                if (sPos.z < 0)
                {
                    //在相机背面
                    inCamBack = true;
                    //类似小孔成像原理，图像会颠倒过来，x、y需要取负号
                    localPoint.x *= -1;
                    localPoint.y *= -1;
                }
                return localPoint;
            }
        }

        //屏幕坐标转ui坐标
        public Vector2 Screen2UIPosition(Vector2 screenPoint, RectTransform rect = null, Camera cam = null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect ? rect : UIRoot as RectTransform,
                screenPoint, cam ? cam : UICamera, out var localPoint);

            return localPoint;
        }

        //屏幕坐标转世界坐标
        public Vector3 Screen2WorldPosition(Vector2 screenPoint, RectTransform rect = null, Camera cam = null)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rect ? rect : UIRoot as RectTransform,
                screenPoint, cam ? cam : UICamera, out var worldPoint);
            return worldPoint;
        }

        //设置当前RectTansfrom缺口
        public void SetNotchTransfrom(RectTransform rectTran)
        {
            Vector2 anchorMin = new Vector2(_curNotchValue / 100, 0);
            Vector2 anchorMax = new Vector2(1 - _curNotchValue / 100, 1);
            rectTran.anchorMin = anchorMin;
            rectTran.anchorMax = anchorMax;
        }
        #endregion
    }

}

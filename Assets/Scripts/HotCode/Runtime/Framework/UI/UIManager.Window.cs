using Cysharp.Threading.Tasks;
using UniFan;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniFan.Res;

namespace HotCode.Framework
{
    public partial class UIManager : ManagerSingleton<UIManager>
    {
        //所有的已经打开了的UI窗体
        public Dictionary<System.Type, UIBaseWindow> _allWindows = new Dictionary<System.Type, UIBaseWindow>();
        //所有的已经打开了的UI窗体，按层级分
        public List<UIBaseWindow>[] _allLayerWindows;
        //所有UI窗体的已经加载完成的
        private Dictionary<System.Type, ResLoader> _allWindowResloader = new Dictionary<System.Type, ResLoader>();
        //异步加载中的UI窗体
        private HashSet<System.Type> _asyncLoading = new HashSet<Type>();

        /// <summary>
        /// 删除UI阶段，不可创建新UI
        /// </summary>
        private bool _isInCloseAll = false;

        public bool InCloseAllWindow => _isInCloseAll;


        #region internal

        private T InitWindowInternal<T>(UIBaseWindow window, GameObject prefab, IUICreateConfig setting) where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            if (prefab == null)
            {
                RemoveWindowInternal(type);
#if UNITY_EDITOR
                Debug.LogWarning($"UIWindow load failed:{type.FullName}");
#endif
                return null;
            }

            GameObject go = UnityEngine.Object.Instantiate(prefab, GetUILayoutLevelRoot(setting.layer));

            go.name = prefab.name;
            var rectTransform = go.transform as RectTransform;

            UICoreHelper.ResetRootTransform(rectTransform);

            var layerWindows = _allLayerWindows[(int)setting.layer];
            int sortOrder = 0;
            if (layerWindows.Count > 0)
            {
                sortOrder = layerWindows[layerWindows.Count - 1].winSetting.sortOrder + UIConstant.SortOrderBetweenUI;
            }

            UICoreHelper.AddWindowCanvas(go, GetUILayoutName(setting.layer), sortOrder);

            //初始化
            window.winSetting.permanent = setting.permanent;
            window.winSetting.layer = setting.layer;
            window.winSetting.sortOrder = sortOrder;
            window.winSetting.resloader = _allWindowResloader[type];
            window.Init(go);

            _allWindows.Add(type, window);
            layerWindows.Add(window);

            return window as T;
        }

        private UIBaseWindow CreateWindowInernal<T>(Type type, ref IUICreateConfig setting, out string realPath, out ResLoader resloader) where T : UIBaseWindow, new()
        {
            realPath = null;
            resloader = null;
            if (_isInCloseAll)
            {
                Debug.LogError($"Cant create window while in deleting all window: {type.FullName}");
                return null;
            }
            T window = new T();
            if (setting == null)
            {
                setting = window.createConfig;
            }
#if UNITY_EDITOR
            if (setting == null)
            {
                Debug.LogError($"UI Window Create Setting is null:{type.FullName}");
                return null;
            }
#endif
            if (!_allWindowResloader.TryGetValue(type, out resloader))
            {
                resloader = ResLoader.Create();
                _allWindowResloader[type] = resloader;
            }
            string prefabPath = setting.prefabName;
            if (!string.IsNullOrEmpty(setting.parentPath))
            {
                prefabPath = setting.parentPath + "/" + prefabPath;
            }
            realPath = prefabPath;
            return window;
        }

        /// <summary>
        /// 内部接口，移除window
        /// </summary>
        /// <param name="type"></param>
        private void RemoveWindowInternal(Type type)
        {
            if (_allWindowResloader.TryGetValue(type, out var resloader))
            {
                _allWindowResloader.Remove(type);
                if (resloader != null)
                {
                    resloader.Put2Pool();
                }
                if (_allWindows.TryGetValue(type, out var window))
                {
                    _allLayerWindows[(int)window.winSetting.layer].Remove(window);
                    window.Dispose();
                    _allWindows.Remove(type);
                }
            }
        }



        private void AddAsyncLoading(Type type)
        {
            //异步加载中的
            if (!_asyncLoading.Contains(type))
            {
                _asyncLoading.Add(type);
            }
            //添加遮罩
            MaxMaskActive = true;
        }

        private void RemoveAsyncLoading(Type type)
        {
            _asyncLoading.Remove(type);
            if (_asyncLoading.Count == 0)
            {
                //删除遮罩
                MaxMaskActive = false;
            }
        }

        private void ClearAllAsyncLoading()
        {
            foreach (var type in _asyncLoading)
            {
                if (_allWindowResloader.TryGetValue(type, out var resloader))
                {
                    _allWindowResloader.Remove(type);
                    if (resloader != null)
                    {
                        resloader.Put2Pool();
                    }
                }
            }
            _asyncLoading.Clear();
            //删除遮罩
            MaxMaskActive = false;
        }
        #endregion

        #region public window function

        public T GetWindow<T>() where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            return GetWindow(type) as T;
        }

        public UIBaseWindow GetWindow(Type type)
        {
            if (_allWindows.TryGetValue(type, out var window))
            {
                return window;
            }
            return null;
        }


        public T CreateWindow<T>(IUICreateConfig setting = null) where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            var window = GetWindow(type);
            if (window != null)
            {
                return window as T;
            }
            window = CreateWindowInernal<T>(type, ref setting, out var preafbPath, out var resloader);
            if (window == null)
            {
                return null;
            }
            if (setting.blurConfig != null)
            {
                Debug.LogWarning("同步加载不支持模糊背景!");
            }
            var prefab = resloader.LoadABAsset<GameObject>(PathConstant.GetUIPrefabPath(preafbPath));
            return InitWindowInternal<T>(window, prefab, setting);
        }

        /// <summary>
        /// 纯创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async UniTask<T> CreateWindowAsync<T>(IUICreateConfig setting = null) where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            var window = GetWindow(type);
            if (window != null)
            {
                return window as T;
            }
            window = CreateWindowInernal<T>(type, ref setting, out var preafbPath, out var resloader);
            if (window == null)
            {
                return null;
            }

            //RenderTexture blurRt = null;
            //if (setting.blurConfig != null)
            //{
            //    if (CVolumeManager.instance.TryGetVolumeComponent(CPostProcessingUtil.GenVolumeComponentId("UICamera", UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing, nameof(BlurVolume)), out var cv) && cv is BlurVolume bv)
            //    {
            //        int width = (int)(this.BackgroundStretchSize.x / setting.blurConfig.downSampling);
            //        int height = (int)(this.BackgroundStretchSize.y / setting.blurConfig.downSampling);
            //        blurRt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            //        bv.SetRenderTexureTarget(blurRt);
            //        await UniTask.Yield();
            //        bv.ClearRenderTextureTarget();
            //    }
            //    else
            //    {
            //        LogGame.LogWarning("Cant't find UICamera Blur Component");
            //    }
            //}


            //异步加载中的
            AddAsyncLoading(type);
            var prefab = await resloader.LoadABAssetAwait<GameObject>(PathConstant.GetUIPrefabPath(preafbPath));
            RemoveAsyncLoading(type);

            var result = InitWindowInternal<T>(window, prefab, setting);

            //添加模糊效果
            //if (blurRt != null)
            //{
            //    var blackGround = window.gameObject.GetComponentInChildren<UIBackgroundAdapter>();
            //    if (blackGround != null)
            //    {
            //        blackGround.SetBGBlurTexture(blurRt);
            //    }
            //    else
            //    {
            //        UnityEngine.Object.Destroy(blurRt);
            //    }
            //}

            return result;
        }

        /// <summary>
        /// 创建或者显示Window
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async UniTask<T> ShowWindowAsync<T>(IUICreateConfig setting = null) where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            var window = GetWindow(type);
            if (window != null)
            {
                return window as T;
            }
            T result = await CreateWindowAsync<T>(setting);
            if (result != null)
            {
                result.Show();
            }
            return result;
        }

        public T ShowWindow<T>(IUICreateConfig setting = null) where T : UIBaseWindow, new()
        {
            var type = typeof(T);
            var window = GetWindow(type);
            if (window != null)
            {
                return window as T;
            }
            T result = CreateWindow<T>(setting);
            if (result != null)
            {
                result.Show();
            }
            return result;
        }

        /// <summary>
        ///   隐藏一个界面，没有则无视
        /// </summary>
        public void HideWindow<T>() where T : UIBaseWindow, new()
        {
            T win = GetWindow<T>();
            if (win != null)
            {
                win.Hide();
            }
        }

        public void HideWindow(Type type)
        {
            UIBaseWindow win = GetWindow(type);
            if (win != null)
            {
                win.Hide();
            }
        }

        public void CloseWindow(Type type)
        {
            var window = GetWindow(type);
            if (window != null)
            {
                RemoveWindowInternal(type);
            }
            RemoveAsyncLoading(type);
        }


        public void CloseWindow<T>()
        {
            var type = typeof(T);
            var window = GetWindow(type);
            if (window != null)
            {
                RemoveWindowInternal(type);
            }
            RemoveAsyncLoading(type);
        }

        /// <summary>
        /// 关闭所有UI窗体
        /// </summary>
        /// <param name="containPermanent">是否包含常驻UI</param>
        public void CloseAllWindow(bool containPermanent)
        {
            _isInCloseAll = true;
            //清除所有的异步加载中的UI
            ClearAllAsyncLoading();

            var windows = ListPool<UIBaseWindow>.Get();
            foreach (var kv in _allWindows)
            {
                windows.Add(kv.Value);
            }

            foreach (var window in windows)
            {
                if (containPermanent || !window.winSetting.permanent)
                {
                    RemoveWindowInternal(window.GetType());
                }
            }

            ListPool<UIBaseWindow>.Put(windows);

            _isInCloseAll = false;
        }

        /// <summary>
        /// 隐藏所有UI窗体
        /// </summary>
        /// <param name="containPermanent">是否包含常驻UI</param>
        public void HideAllWindow(bool containPermanent)
        {
            var windows = ListPool<UIBaseWindow>.Get();
            foreach (var kv in _allWindows)
            {
                windows.Add(kv.Value);
            }

            foreach (var window in windows)
            {
                if (window.active
                    && (containPermanent || !window.winSetting.permanent)
                    )
                {
                    window.Hide();
                }
            }

            ListPool<UIBaseWindow>.Put(windows);
        }

        #endregion

    }
}

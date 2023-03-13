using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UniFan.Res
{
    public class ResLoader : IResLoader
    {
        private HashSet<IRes> _resSet = new HashSet<IRes>();

        private Dictionary<AsyncTaskSequence, Action<Object>> _asyncLoadMap = new Dictionary<AsyncTaskSequence, Action<Object>>();

        /// <summary>
        /// 解决SpriteAtlas调用GetSprite内存泄露问题
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Sprite GetAtlasSprite(SpriteAtlas atlas, string spriteName)
        {
            return ABAssetRes.GetAbAssetAtlasSprite(atlas, spriteName);
        }

        //添加资源的引用计数
        public void AddResRefCount(IRes res)
        {
            if (res == null)
            {
                return;
            }
            if (!_resSet.Contains(res))
            {
                _resSet.Add(res);
                res.Retain();
            }
        }

        public T LoadABAsset<T>(string assetName) where T : Object
        {
            return Load<T>(assetName, ResType.ABAsset);
        }

        public Object LoadABAsset(string assetName)
        {
            return Load(assetName, ResType.ABAsset);
        }

        public AssetBundle LoadAssetBundle(string bundleName)
        {
            return Load<AssetBundle>(bundleName.ToLower(), ResType.AssetBundle);
        }

        public T LoadResourceAsset<T>(string assetName) where T : Object
        {
            return Load<T>(assetName, ResType.Resource);
        }

        public Object LoadResourceAsset(string assetName)
        {
            return Load(assetName, ResType.Resource);
        }

        public T Load<T>(string assetName, ResType resType) where T : Object
        {
            Object asset = DoLoad(assetName, resType);
            return asset as T;
        }

        public Object Load(string assetName, ResType resType)
        {
            Object asset = DoLoad(assetName, resType);
            return asset;
        }

        private Object DoLoad(string assetName, ResType resType)
        {
#if UNITY_EDITOR
            float t = Time.realtimeSinceStartup;
#endif

            IRes res = ResManager.Instance.GetRes(assetName, resType, true);
            if (res == null)
            {
                Debug.LogError("Failed to create Res:" + assetName + " type:" + resType.ToString());
                return null;
            }
            AddResRefCount(res);
            StartLoad(res);

            if (res.Asset == null)
            {
#if UNITY_EDITOR
                if (!ResManager.EditorBundleMode && resType == ResType.AssetBundle)
                {
                }
                else
#endif
                {
                    Debug.LogError("Failed to Load Res:" + assetName + " type:" + resType.ToString());
                }
                return null;
            }
#if UNITY_EDITOR
            //Debug.Log(assetName + "_" + (Time.realtimeSinceStartup - t).ToString());
#endif
            return res.Asset;
        }

        public void StartLoad(IRes res)
        {
            var depends = res.GetAndRetainDependResList();

            if (depends != null)
            {
                foreach (var depend in depends)
                {
                    if (depend == null)
                    {
                        continue;
                    }
                    StartLoad(depend);
                }
            }
            res.Load();
        }


        public void LoadABAssetAsync(string assetName, Action<Object> loadCompleteCallback)
        {
            DoLoadAsync(assetName, ResType.ABAsset, loadCompleteCallback);
        }

        public void LoadAssetBundleAsync(string bundleName, Action<Object> loadCompleteCallback)
        {
            DoLoadAsync(bundleName.ToLower(), ResType.AssetBundle, loadCompleteCallback);
        }

        public void LoadResourceAsset(string assetName, Action<Object> loadCompleteCallback)
        {
            DoLoadAsync(assetName, ResType.Resource, loadCompleteCallback);
        }

        private void DoLoadAsync(string assetName, ResType resType, Action<Object> loadCompleteCallback)
        {
#if UNITY_EDITOR
            float t = Time.realtimeSinceStartup;
#endif
            IRes res = ResManager.Instance.GetRes(assetName, resType, true);
            if (res == null)
            {
                if (loadCompleteCallback != null)
                {
                    loadCompleteCallback(null);
                }
                return;
            }
            AsyncTaskSequence asyncTask = AsyncTaskSequence.Create();
            AddResRefCount(res);
            Add2AsyncLoad(res, asyncTask);
            asyncTask.OnAllTaskFinish += OnResLoadFinish;
            asyncTask.Append(res);
            _asyncLoadMap.Add(asyncTask, loadCompleteCallback);
            asyncTask.Start();
#if UNITY_EDITOR
            //Debug.Log(assetName + "_" + (Time.realtimeSinceStartup - t).ToString());
#endif
        }

        private void Add2AsyncLoad(IRes res, AsyncTaskSequence asyncTask)
        {
            var depends = res.GetAndRetainDependResList();

            if (depends != null)
            {
                bool isfirst = true;
                foreach (var depend in depends)
                {
                    if (depend == null)
                    {
                        continue;
                    }
                    Add2AsyncLoad(depend, asyncTask);
                    if (isfirst)
                    {
                        isfirst = false;
                        asyncTask.Append(depend);
                    }
                    else
                    {
                        asyncTask.Join(depend);
                    }
                }
            }
        }

        private void OnResLoadFinish(AsyncTaskSequence asyncTask)
        {
            if (_asyncLoadMap.ContainsKey(asyncTask))
            {
                Action<Object> assetCallback = _asyncLoadMap[asyncTask];
                if (assetCallback != null)
                {
                    List<IAsyncTask> lastTask = asyncTask.GetLastSequence();
                    if (lastTask.Count > 0)
                    {
                        IRes res = lastTask[0] as IRes;
                        if (res != null)
                        {
                            assetCallback(res.Asset);
                        }
                        else
                        {
                            assetCallback(null);
                        }
                    }
                    else
                    {
                        assetCallback(null);
                    }
                }
                if (_asyncLoadMap.ContainsKey(asyncTask))
                {
                    _asyncLoadMap.Remove(asyncTask);
                    asyncTask.Put2Pool();
                }
            }
        }

        public AsyncWait LoadABAssetAsyncAwait(string assetName, Action<Object> loadCompleteCallback = null)
        {
            return DoLoadAsyncAwait(assetName, ResType.ABAsset, loadCompleteCallback);
        }

        public AsyncWait LoadAssetBundleAsyncAwait(string bundleName, Action<Object> loadCompleteCallback = null)
        {
            return DoLoadAsyncAwait(bundleName.ToLower(), ResType.AssetBundle, loadCompleteCallback);
        }

        public AsyncWait LoadResourceAssetAwait(string assetName, Action<Object> loadCompleteCallback = null)
        {
            return DoLoadAsyncAwait(assetName, ResType.Resource, loadCompleteCallback);
        }

        private AsyncWait DoLoadAsyncAwait(string assetName, ResType resType, Action<Object> loadCompleteCallback)
        {

            AsyncWait wait = new AsyncWait();

            IRes res = ResManager.Instance.GetRes(assetName, resType, true);
            if (res == null)
            {
                if (loadCompleteCallback != null)
                {
                    loadCompleteCallback(null);
                }
                wait.IsDone = true;
                return wait;
            }
            AsyncTaskSequence asyncTask = AsyncTaskSequence.Create();
            AddResRefCount(res);
            Add2AsyncLoad(res, asyncTask);
            asyncTask.OnAllTaskFinish += (ats) =>
            {
                if (_asyncLoadMap.TryGetValue(ats, out var assetCallback))
                {
                    Object asset = null;
                    List<IAsyncTask> lastTask = ats.GetLastSequence();
                    if (lastTask.Count > 0)
                    {
                        IRes curRes = lastTask[0] as IRes;
                        asset = curRes.Asset;
                    }
                    if (asset == null)
                    {
                        Debug.LogWarning("Resloader DoLoadAsyncAwait Asset is null,assetName:" + assetName);
                    }
                    if (assetCallback != null)
                    {
                        assetCallback(asset);
                    }
                    wait.IsDone = true;
                    wait.Result = asset;
                    if (_asyncLoadMap.ContainsKey(ats))
                    {
                        _asyncLoadMap.Remove(ats);
                        ats.Put2Pool();
                    }
                }
            };
            asyncTask.Append(res);
            _asyncLoadMap.Add(asyncTask, loadCompleteCallback);
            asyncTask.Start();
            return wait;
        }

        public void ReleaseAllRes()
        {
            if (_asyncLoadMap.Count > 0)
            {
                foreach (var item in _asyncLoadMap)
                {
                    AsyncTaskSequence task = item.Key;
                    if (task != null)
                    {
                        task.Put2Pool();
                    }
                }
                _asyncLoadMap.Clear();
            }

            if (_resSet.Count > 0)
            {
                foreach (var item in _resSet)
                {
                    item.Release();
                }
                _resSet.Clear();
                ResManager.NotifyResManagerClear();
            }

        }

        #region Class Pool
        public uint MaxStore => 50;

        public static ResLoader Create()
        {
            ResLoader loader = ClassPool.Get<ResLoader>();
            return loader;
        }

        public void Put2Pool()
        {
            ReleaseAllRes();
            ClassPool.Put<ResLoader>(this);
        }

        public void OnReset()
        {
            _resSet.Clear();
            _asyncLoadMap.Clear();
        }
        #endregion

    }

}
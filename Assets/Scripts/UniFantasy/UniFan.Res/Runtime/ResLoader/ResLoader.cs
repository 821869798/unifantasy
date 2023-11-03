using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

namespace UniFan.Res
{
    public class ResLoader : IResLoader, IDisposable
    {

        private HashSet<IRes> _resSet = new HashSet<IRes>();

        private Dictionary<AsyncTaskSequence, object> _asyncLoadMap = new Dictionary<AsyncTaskSequence, object>();

        /// <summary>
        /// 用来判断是async await的加载任务
        /// </summary>
        private static object _uniTaskObject = new object();

        #region function

        //添加资源的引用计数
        private void AddResRefCount(IRes res)
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

        public void ReleaseAllRes()
        {
            if (_asyncLoadMap.Count > 0)
            {
                foreach (var item in _asyncLoadMap)
                {
                    AsyncTaskSequence task = item.Key;
                    if (task == null)
                    {
                        continue;
                    }
                    if (item.Value == _uniTaskObject)
                    {
                        //这类是Unitask创建的加载，自动完成即可
                        task.CancelAwait();
                    }
                    task.Put2Pool();
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

        #endregion

        #region Load Res Sync

        public T LoadABAsset<T>(string assetName) where T : Object
        {
            return Load<T>(assetName, ResType.ABAsset);
        }

        public Object LoadABAsset(string assetName, Type assetType = null)
        {
            return DoLoad(assetName, ResType.ABAsset, assetType);
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
            Object asset = DoLoad(assetName, resType, typeof(T));
            return asset as T;
        }

        public Object Load(string assetName, ResType resType)
        {
            Object asset = DoLoad(assetName, resType);
            return asset;
        }

        private Object DoLoad(string assetName, ResType resType, Type assetType = null)
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
            if (assetType != null)
            {
                res.InitAssetType(assetType);
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

        #endregion

        #region Load Res Async With Callback

        public void LoadABAssetAsync(string assetName, Action<Object> loadCompleteCallback, Type assetType = null)
        {
            DoLoadAsync(assetName, ResType.ABAsset, loadCompleteCallback, assetType = null);
        }

        public void LoadAssetBundleAsync(string bundleName, Action<Object> loadCompleteCallback)
        {
            DoLoadAsync(bundleName.ToLower(), ResType.AssetBundle, loadCompleteCallback);
        }

        public void LoadResourceAsset(string assetName, Action<Object> loadCompleteCallback)
        {
            DoLoadAsync(assetName, ResType.Resource, loadCompleteCallback);
        }

        private void DoLoadAsync(string assetName, ResType resType, Action<Object> loadCompleteCallback, Type assetType = null)
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
            if (assetType != null)
            {
                res.InitAssetType(assetType);
            }
            AsyncTaskSequence asyncTask = AsyncTaskSequence.Create();
            AddResRefCount(res);
            Add2AsyncLoad(res, asyncTask);
            asyncTask.OnAllTaskFinish += OnResLoadAsyncFinish;
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

        private void OnResLoadAsyncFinish(AsyncTaskSequence asyncTask)
        {
            if (_asyncLoadMap.ContainsKey(asyncTask))
            {
                Action<Object> assetCallback = _asyncLoadMap[asyncTask] as Action<Object>;
                if (assetCallback != null)
                {
                    List<IAsyncTask> lastTask = asyncTask.GetLastSequence();
                    if (lastTask != null && lastTask.Count > 0)
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

        #endregion

        #region Load Res Async await

        public UniTask<T> LoadABAssetAwait<T>(string assetName) where T : Object
        {
            return DoLoadAsyncAwait<T>(assetName, ResType.ABAsset);
        }

        public UniTask<AssetBundle> LoadAssetBundleAwait(string assetName)
        {
            return DoLoadAsyncAwait<AssetBundle>(assetName.ToLower(), ResType.AssetBundle);
        }

        public UniTask<T> LoadResourceAssetAwait<T>(string assetName) where T : Object
        {
            return DoLoadAsyncAwait<T>(assetName, ResType.Resource);
        }

        private async UniTask<T> DoLoadAsyncAwait<T>(string assetName, ResType resType) where T : Object
        {
            IRes res = ResManager.Instance.GetRes(assetName, resType, true);
            if (res == null)
            {
                return null;
            }

            res.InitAssetType(typeof(T));

            //准备加载数据
            AsyncTaskSequence asyncTask = AsyncTaskSequence.Create();
            AddResRefCount(res);
            Add2AsyncLoad(res, asyncTask);
#pragma warning disable CS4014
            asyncTask.Append(res);
            _asyncLoadMap.Add(asyncTask, _uniTaskObject);
            //等待加载完成
            var task = asyncTask.StartAwait();
            asyncTask.Start();
            await task;
#pragma warning restore CS4014


            //移除自己
            if (_asyncLoadMap.ContainsKey(asyncTask))
            {
                _asyncLoadMap.Remove(asyncTask);
            }

            //获取结果
            T result;
            List<IAsyncTask> lastTask = asyncTask.GetLastSequence();
            if (lastTask != null && lastTask.Count > 0)
            {
                IRes curRes = lastTask[0] as IRes;
                result = curRes.Asset as T;
            }
            else
            {
                result = null;
            }

            //清除自己
            asyncTask.Put2Pool();

            return result;
        }

        #endregion 

        #region [Obsolete] Load Res Async With IEnumerator

        //[Obsolete]
        //public AsyncWait LoadABAssetAsyncLagacy(string assetName)
        //{
        //    return DoLoadAsyncLagacy(assetName, ResType.ABAsset, null);
        //}

        //[Obsolete]
        //public AsyncWait LoadAssetBundleAsyncLagacy(string bundleName)
        //{
        //    return DoLoadAsyncLagacy(bundleName.ToLower(), ResType.AssetBundle, null);
        //}

        //[Obsolete]
        //public AsyncWait LoadResourceAssetAsyncLagacy(string assetName)
        //{
        //    return DoLoadAsyncLagacy(assetName, ResType.Resource, null);
        //}

        [Obsolete]
        public AsyncWait DoLoadAsyncLagacy(string assetName, ResType resType, Action<Object> loadCompleteCallback, Type assetType = null)
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
            if (assetType != null)
            {
                res.InitAssetType(assetType);
            }
            AsyncTaskSequence asyncTask = AsyncTaskSequence.Create();
            AddResRefCount(res);
            Add2AsyncLoad(res, asyncTask);
            asyncTask.OnAllTaskFinish += (ats) =>
            {
                if (_asyncLoadMap.TryGetValue(ats, out var obj))
                {
                    Object asset = null;
                    Action<Object> assetCallback = obj as Action<Object>;
                    List<IAsyncTask> lastTask = ats.GetLastSequence();
                    if (lastTask != null && lastTask.Count > 0)
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

        #endregion

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


        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放非托管资源
        /// </summary>
        /// <param name="disposing">disposing表示是否调用其他Dispose</param>
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseAllRes();
                return;
            }

            if (_resSet.Count > 0)
            {
#if !UNITY_EDITOR
                Debug.LogError("[ResLoader|Dispose] No manual release resource,resCount:" + _resSet.Count);
#endif
                // 析构函数可能调用是子线程中，需要放入主线程中调用Unity相关api
                ResManager.mainThreadActionQue.Enqueue(() => ReleaseAllRes());
            }
        }

        ~ResLoader()
        {
            Dispose(false);
        }
        #endregion

    }

}
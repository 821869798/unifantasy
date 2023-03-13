using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFan.Res
{
    public class AssetBundleRes : Res
    {
        public override ResType ResType => ResType.AssetBundle;

        private List<IRes> _depBundleList = new List<IRes>();
        //是否引用了依赖
        private bool _hasRetainDep = false;

        private AssetBundleCreateRequest _assetBundleCreateRequest;

        public AssetBundle AssetBundle
        {
            get { return Asset as AssetBundle; }
            private set { Asset = value; }
        }

        public override List<IRes> GetAndRetainDependResList()
        {
            if (_hasRetainDep)
            {
                return _depBundleList;
            }
            _hasRetainDep = true;
#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                return _depBundleList;
            }
#endif
            if (ResManager.Instance.ContainsBundle(AssetName))
            {
                List<string> depNameList = ListPool<string>.Get();
                ResManager.Instance.GetBundleDependences(AssetName, depNameList);
                for (int i = 0; i < depNameList.Count; i++)
                {
                    string depBundleName = depNameList[i];
                    if (!string.IsNullOrEmpty(depBundleName))
                    {
                        IRes bundleRes = ResManager.Instance.GetRes(depBundleName, ResType.AssetBundle, true);
                        bundleRes.Retain();
                        this._depBundleList.Add(bundleRes);
                    }
                }
                ListPool<string>.Put(depNameList);
            }
            return _depBundleList;
        }

        public static AssetBundleRes Create(string assetName)
        {
            AssetBundleRes res = ClassPool.Get<AssetBundleRes>();
            res.AssetName = assetName;
            return res;
        }

        public override void OnReset()
        {
            _depBundleList.Clear();
            _assetBundleCreateRequest = null;
            _hasRetainDep = false;
            base.OnReset();
        }

        public override void Put2Pool()
        {
            ClassPool.Put<AssetBundleRes>(this);
        }

        public override bool Load()
        {
            if (!CheckLoadAble())
            {
                WarnCancelSyncLoad();
                return false;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                return false;
            }

#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                return true;
            }
            else
#endif
            {
                if (!ResManager.Instance.ContainsBundle(AssetName))
                {
                    Debug.LogError("AssetBundle Load Fail,can't find name:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }

                State = ResState.Loading;

                var filePath = FilePathHelper.Instance.GetBundlePath(AssetName, out var pathType);
                AssetBundle ab;
                ab = AssetBundle.LoadFromFile(filePath);

                if (ab == null)
                {
                    Debug.LogError("Load AssetBundle is null:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }

                Asset = ab;

                State = ResState.Ready;
            }
            return true;
        }

        public override void LoadAsync()
        {
            if (!CheckLoadAble())
            {
                return;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                OnResLoadFaild();
                return;
            }

            State = ResState.Loading;

            ResManager.Instance.PushIEnumeratorTask(this);
        }

        public override IEnumerator DoIEnumeratorTask(System.Action finishCallback)
        {
            //开启的时候已经结束了
            if (RefCount <= 0)
            {
                OnResLoadFaild();
                finishCallback();
                yield break;
            }

#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                if (AssetBundleUtility.SimulationAsyncLoad)
                {
                    //编辑器下模拟异步加载
                    float time = UnityEngine.Random.Range(0, 0.1f) + 0.1f;
                    yield return new WaitForSecondsRealtime(time);
                }


                State = ResState.Ready;
                finishCallback();
                yield break;
            }
            else
#endif
            {
                if (!ResManager.Instance.ContainsBundle(AssetName))
                {
                    Debug.LogError("AssetBundle Load Fail,can't find name:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }

                var filePath = FilePathHelper.Instance.GetBundlePath(AssetName, out var pathType);
                AssetBundleCreateRequest abCreateRequest;
                abCreateRequest = AssetBundle.LoadFromFileAsync(filePath);

                _assetBundleCreateRequest = abCreateRequest;
                yield return abCreateRequest;
                _assetBundleCreateRequest = null;

                if (!abCreateRequest.isDone)
                {
                    Debug.LogError("AssetBundleCreateRequest Not Done:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }

                AssetBundle = abCreateRequest.assetBundle;
                if (AssetBundle == null)
                {
                    Debug.LogWarning("AssetBundleRes Load Finish with null:" + this.AssetName);
                }
            }

            State = ResState.Ready;
            finishCallback();
            if (RefCount <= 0)
            {
                ResManager.NotifyResManagerClear();
            }
        }

        protected override float CalculateProgress()
        {

#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                return 1;
            }
#endif
            if (_assetBundleCreateRequest == null)
            {
                return 0;
            }

            return _assetBundleCreateRequest.progress;
        }

        protected override void OnZeroRef()
        {

        }

        protected override void OnReleaseRes()
        {
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
                AssetBundle = null;
            }

            if (_depBundleList.Count > 0)
            {
                for (int i = 0; i < _depBundleList.Count; i++)
                {
                    _depBundleList[i].Release();
                }
                _depBundleList.Clear();
                ResManager.NotifyResManagerClear();
            }
            _hasRetainDep = false;
            State = ResState.Waiting;

        }
    }


}


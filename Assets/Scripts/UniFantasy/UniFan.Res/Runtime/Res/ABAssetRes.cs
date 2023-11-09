using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UniFan.Res
{
    public class ABAssetRes : Res
    {
        public override ResType ResType => ResType.ABAsset;

        private string _ownerBundleName = string.Empty;

        public string OwnerBundleName
        {
            protected set { this._ownerBundleName = value; }
            get { return this._ownerBundleName; }
        }

        private List<IRes> _depBundleList = new List<IRes>();

        //是否引用了依赖
        private bool _hasRetainDep = false;

        protected AssetBundleRequest _assetBundleRequest;

        public override Object Asset
        {
            get => base.Asset;
            protected set
            {
                base.Asset = value;
            }
        }

        public Type AssetType { private set; get; }

        public static ABAssetRes Create(string assetName)
        {
            ABAssetRes res = ClassPool.Get<ABAssetRes>();
            res.AssetName = assetName;
#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {

            }
            else
#endif
            {
                if (ResManager.Instance.ContainsAsset(res.AssetName))
                {
                    res.OwnerBundleName = ResManager.Instance.GetBundleName(res.AssetName);
                }
            }
            return res;
        }

        public override void OnReset()
        {
            _depBundleList.Clear();
            _assetBundleRequest = null;
            OwnerBundleName = string.Empty;
            _hasRetainDep = false;
            AssetType = null;
            base.OnReset();
        }

        public override void Put2Pool()
        {
            ClassPool.Put<ABAssetRes>(this);
        }

        public override List<IRes> GetAndRetainDependResList()
        {
#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {

            }
            else
#endif
            {
                if (!_hasRetainDep)
                {
                    _hasRetainDep = true;
                    if (!string.IsNullOrEmpty(OwnerBundleName))
                    {
                        IRes bundleRes = ResManager.Instance.GetRes(OwnerBundleName, ResType.AssetBundle, true);
                        bundleRes.Retain();
                        _depBundleList.Add(bundleRes);
                    }
                }
            }
            return _depBundleList;
        }

        public override void InitAssetType(Type resType)
        {
            this.AssetType = resType;
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
                State = ResState.Loading;
                string assetPath = FilePathHelper.Instance.GetEditorAssetPath(AssetName);
                Object obj;
                if (AssetType != null)
                {
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, AssetType);
                }
                else
                {
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                }

                if (obj == null)
                {
                    Debug.LogError("Failed Load Asset:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }
                else
                {
                    var realAssetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
                    if (realAssetPath != assetPath)
                    {
                        Debug.LogError("Failed Load Asset, Case Inconsistency :" + AssetName);
                        OnResLoadFaild();
                        return false;
                    }
                }

                Asset = obj;
            }
            else
#endif
            {
                if (_depBundleList.Count <= 0)
                {
                    Debug.LogError("Asset has not ab dep:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }
                AssetBundleRes bundleRes = _depBundleList[0] as AssetBundleRes;
                if (bundleRes == null)
                {
                    Debug.LogError("BundleRes is null,Load Asset is:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }
                State = ResState.Loading;
                string shortAssetName = FilePathHelper.Instance.GetAssetNameInBundle(AssetName);
                AssetBundle ab = bundleRes.AssetBundle;
                if (ab == null)
                {
                    Debug.LogError("Fail Load Asset,AssetBundle is null:" + bundleRes.AssetName);
                    OnResLoadFaild();
                    return false;
                }
                Object obj;
                if (AssetType != null)
                {
                    obj = ab.LoadAsset(shortAssetName, AssetType);
                }
                else
                {
                    obj = ab.LoadAsset(shortAssetName);
                }


                if (obj == null)
                {
                    Debug.LogError("Failed Load ABAsset:" + AssetName);
                    OnResLoadFaild();
                    return false;
                }

                Asset = obj;
            }

            State = ResState.Ready;
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

        public override IEnumerator DoIEnumeratorTask(Action finishCallback)
        {
            if (RefCount <= 0)
            {
                OnResLoadFaild();
                finishCallback();
                yield break;
            }
#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                string assetPath = FilePathHelper.Instance.GetEditorAssetPath(AssetName);
                Object obj;
                if (AssetType != null)
                {
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, AssetType);
                }
                else
                {
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                }

                if (AssetBundleUtility.SimulationAsyncLoad)
                {
                    //编辑器下模拟异步加载
                    float time = UnityEngine.Random.Range(0, 0.3f) + 0.2f;
                    yield return new WaitForSecondsRealtime(time);
                }

                if (obj == null)
                {
                    Debug.LogError("Failed Load Asset:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }
                else
                {
                    var realAssetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
                    if (realAssetPath != assetPath)
                    {
                        Debug.LogError("Failed Load Asset, Case Inconsistency :" + AssetName);
                        OnResLoadFaild();
                        finishCallback();
                        yield break;
                    }
                }
                Asset = obj;
            }
            else
#endif
            {
                if (_depBundleList.Count <= 0)
                {
                    Debug.LogError("Asset has not ab dep:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }
                AssetBundleRes bundleRes = _depBundleList[0] as AssetBundleRes;
                if (bundleRes == null)
                {
                    Debug.LogError("BundleRes is null,Load Asset is:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }

                string assetNameInAB = FilePathHelper.Instance.GetAssetNameInBundle(AssetName);
                AssetBundle ab = bundleRes.AssetBundle;
                if (ab == null)
                {
                    Debug.LogError("Fail Load Asset,AssetBundle is null:" + bundleRes.AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }
                AssetBundleRequest assetRequest;
                if (AssetType != null)
                {
                    assetRequest = ab.LoadAssetAsync(assetNameInAB, AssetType);
                }
                else
                {
                    assetRequest = ab.LoadAssetAsync(assetNameInAB);
                }

                _assetBundleRequest = assetRequest;
                yield return assetRequest;
                _assetBundleRequest = null;

                if (!assetRequest.isDone)
                {
                    Debug.LogError("AssetRequest Not Done:" + AssetName);
                    OnResLoadFaild();
                    finishCallback();
                    yield break;
                }

                Asset = assetRequest.asset;
            }

            State = ResState.Ready;

            finishCallback();

            if (RefCount <= 0)
            {
                OnReleaseRes();
                ResManager.NotifyResManagerClear();
            }
        }

        protected override float CalculateProgress()
        {
            if (_assetBundleRequest == null)
            {
                return 0;
            }

            return _assetBundleRequest.progress;
        }

        protected override void OnReleaseRes()
        {
            if (Asset != null)
            {
                if (Asset is GameObject)
                {
                }
                //SpriteAtals不能删,因为有用到Late Binding,用AB引用计数自动删掉
                else if (Asset is SpriteAtlas)
                {
                }
                else
                {
                    Resources.UnloadAsset(Asset);
                }

                Asset = null;
            }

            if (_depBundleList.Count > 0)
            {
                for (int i = 0; i < _depBundleList.Count; i++)
                {
                    _depBundleList[i].Release();
                }
                _depBundleList.Clear();
            }
            _hasRetainDep = false;
            State = ResState.Waiting;
        }
    }

}


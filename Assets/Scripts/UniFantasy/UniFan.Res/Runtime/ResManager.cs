using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniFan.Res
{

    public class ResManager : ManagerSingleton<ResManager>
    {
        public override int managerPriority => 10;

        private ResManifest _resManifest;

#if UNITY_EDITOR
        public static bool EditorBundleMode = false;
#endif
        //用于存储所有的资源
        private Dictionary<string, IRes>[] _allResMap;
        private List<IRes> _allResList;

        [SerializeField] private int _curCoroutineCount;
        private const int MaxCoroutineCount = 8; //最快协成大概在6到8之间
        private LinkedList<IEnumeratorTask> _resIEnumeratorTaskStack = new LinkedList<IEnumeratorTask>();

        //Res 在ResMgr中 删除的问题，ResMgr定时收集列表中的Res然后删除
        private bool _isResMapDirty;

        /// <summary>
        /// 用于监听资源释放
        /// </summary>
        static internal event Action _notifyResManagerClear;

        // 线程安全队列
        static internal System.Collections.Concurrent.ConcurrentQueue<Action> mainThreadActionQue = new System.Collections.Concurrent.ConcurrentQueue<Action>();
        protected override void InitManager()
        {
            _notifyResManagerClear = this.ClearOnUpdate;

            _allResList = new List<IRes>();
            _allResMap = new Dictionary<string, IRes>[System.Enum.GetNames(typeof(ResType)).Length];
            for (int i = 0; i < _allResMap.Length; i++)
            {
                _allResMap[i] = new Dictionary<string, IRes>();
            }

#if UNITY_EDITOR
            EditorBundleMode = AssetBundleUtility.ActiveBundleMode;
#endif

            AssetBundleUtility.SetAssetBundleDecryptKey(AssetBundleUtility.GetAssetBundleKey());

            //使用include in build,不需要监听atlasRequested
            //SpriteAtlasManager.atlasRequested += AtlasRequested;
        }

        public bool InitAssetBundle()
        {
#if UNITY_EDITOR
            if (!EditorBundleMode)
                return true;
#endif
            //加载AssetBundle信息
            _resManifest = new ResManifest();
            AssetBundle bundle;
            var filePath = FilePathHelper.Instance.GetBundlePath(ResPathConsts.AssetbundleLoadPath, ResPathConsts.ResManifestFilePath, out _);
            bundle = AssetBundle.LoadFromFile(filePath);

            if (bundle == null)
            {
                return false;
            }
            TextAsset ta = bundle.LoadAsset<TextAsset>(ResPathConsts.ResManifestBinaryConfigName);
            if (ta == null)
            {
                bundle.Unload(true);
                return false;
            }
            using (MemoryStream ms = new MemoryStream(ta.bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    _resManifest.Load(br);
                    br.Close();
                }
            }
            bundle.Unload(true);
            return true;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_isResMapDirty)
            {
                RemoveUnusedRes();
            }
            while (mainThreadActionQue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public IRes GetRes(string assetName, ResType resType, bool CreateNew = false)
        {
            IRes res = null;
            int index = (int)resType;
            if (_allResMap[index].TryGetValue(assetName, out res))
            {
                return res;
            }
            if (CreateNew)
            {
                res = ResFactory.Create(assetName, resType);
                if (res != null)
                {
                    _allResMap[index].Add(assetName, res);
                    _allResList.Add(res);
                }
            }
            return res;
        }

        private void OnIEnumeratorTaskFinish()
        {
            --_curCoroutineCount;
            TryStartNextIEnumeratorTask();
        }

        public void PushIEnumeratorTask(IEnumeratorTask task)
        {
            if (task == null)
            {
                return;
            }

            _resIEnumeratorTaskStack.AddLast(task);
            TryStartNextIEnumeratorTask();
        }

        private void TryStartNextIEnumeratorTask()
        {
            if (_resIEnumeratorTaskStack.Count == 0)
            {
                return;
            }

            if (_curCoroutineCount >= MaxCoroutineCount)
            {
                return;
            }

            var task = _resIEnumeratorTaskStack.First.Value;
            _resIEnumeratorTaskStack.RemoveFirst();

            ++_curCoroutineCount;
            MonoDriver.Instance.StartCoroutine(task.DoIEnumeratorTask(OnIEnumeratorTaskFinish));
        }

        static public void NotifyResManagerClear()
        {
            _notifyResManagerClear.Invoke();
        }

        private void ClearOnUpdate()
        {
            _isResMapDirty = true;
        }

        public void RemoveUnusedRes()
        {
            if (!_isResMapDirty)
            {
                return;
            }

            _isResMapDirty = false;

            for (var i = _allResList.Count - 1; i >= 0; --i)
            {
                var res = _allResList[i];
                if (res.RefCount <= 0 && res.State != ResState.Loading)
                {
                    if (res.ReleaseRes())
                    {
                        int index = (int)res.ResType;
                        _allResList.RemoveAt(i);
                        _allResMap[index].Remove(res.AssetName);
                        res.Put2Pool();
                    }
                }
            }

            RemoveUnusedRes();
        }

        #region AssetBundle Info

        public string GetBundleName(string assetPath)
        {
            return _resManifest.GetBundleName(assetPath);
        }

        public bool ContainsBundle(string bundle)
        {
            return _resManifest.ContainsBundle(bundle);
        }

        public bool ContainsAsset(string assetPath)
        {
#if UNITY_EDITOR
            if (!EditorBundleMode)
            {
                return File.Exists("Assets/" + assetPath);
            }
            else
#endif
            {
                return _resManifest.ContainsAsset(assetPath);
            }
        }

        public void GetBundleDependences(string bundleName, List<string> depsList)
        {
            _resManifest.GetBundleDependences(bundleName, depsList);
        }

        #endregion
    }
}
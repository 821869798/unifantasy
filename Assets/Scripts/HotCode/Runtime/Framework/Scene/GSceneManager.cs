using Cysharp.Threading.Tasks;
using UniFan;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UniFan.Res;

namespace HotCode.Framework
{
    public class GSceneManager : ManagerSingleton<GSceneManager>
    {
        /// <summary>
        /// 纯场景加载占比
        /// </summary>
        public const float SceneLoaderRatio = 0.6f;

        /// <summary>
        /// 对应加载出来的每一个场景
        /// </summary>
        public class GSceneUnit
        {
            public string sceneName { internal set; get; }
            public Scene scene { internal set; get; }
            public LoadSceneMode loadSceneMode { internal set; get; }

            internal GSceneUnit(Scene scene, LoadSceneMode loadSceneMode)
            {
                this.sceneName = scene.name;
                this.scene = scene;
                this.loadSceneMode = loadSceneMode;
            }
        }

        protected Dictionary<Scene, GSceneUnit> _allSceneUnits { get; } = new Dictionary<Scene, GSceneUnit>();
        protected Dictionary<string, int> _sceneNameCount { get; } = new Dictionary<string, int>();
        protected Dictionary<string, ResLoader> _sceneResloaders { get; } = new Dictionary<string, ResLoader>();

        /// <summary>
        /// 默认的加载处理器,可以使用自定义的类替换
        /// </summary>
        public static ISceneLoaderProcess defaultLoaderProcess { get; set; } = new SceneLoaderProcessDefault();

        public override int managerPriority => throw new NotImplementedException();

        public event Action<GSceneUnit> onSceneLoaded;
        public event Action<GSceneUnit> onSceneUnLoaded;

        #region internal

        protected override void InitManager()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnLoaded;

            var sceneUnit = new GSceneUnit(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            _allSceneUnits.Add(sceneUnit.scene, sceneUnit);
        }


        /// <summary>
        /// 获取场景的Resloader
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        protected ResLoader AddSceneWithResloader(string sceneName)
        {
            if (_sceneNameCount.TryGetValue(sceneName, out var count))
            {
                _sceneNameCount[sceneName]++;
            }
            _sceneNameCount[sceneName] = 1;
            if (_sceneResloaders.TryGetValue(sceneName, out var resloader))
            {
                return resloader;
            }
            resloader = ResLoader.Create();
            _sceneResloaders[sceneName] = resloader;
            return resloader;
        }

        protected void AddSceneOnly(string sceneName)
        {
            if (_sceneNameCount.TryGetValue(sceneName, out var count))
            {
                _sceneNameCount[sceneName] = count + 1;
                return;
            }
            _sceneNameCount[sceneName] = 1;
        }

        /// <summary>
        /// 尝试释放Resloader
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        protected void RemoveSceneAndResloader(string sceneName)
        {
            if (!_sceneNameCount.TryGetValue(sceneName, out var count))
            {
                return;
            }
            if (count <= 1)
            {
                _sceneNameCount.Remove(sceneName);
                if (_sceneResloaders.TryGetValue(sceneName, out var resloader))
                {
                    resloader.Put2Pool();
                    _sceneResloaders.Remove(sceneName);
                }
            }
            else
            {
                count--;
                _sceneNameCount[sceneName] = count;
            }
        }

        protected void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            //Debug.LogError($"[{Time.frameCount}]SceneLoaded:" + scene.name);

            GSceneUnit sceneUnit = new GSceneUnit(scene, loadSceneMode);
            _allSceneUnits.Add(sceneUnit.scene, sceneUnit);

            onSceneLoaded?.Invoke(sceneUnit);
        }

        protected void SceneUnLoaded(Scene scene)
        {
            //Debug.LogError($"[{Time.frameCount}]SceneUnLoaded:" + scene.name);

            if (_allSceneUnits.TryGetValue(scene, out var sceneUnit))
            {
                RemoveSceneAndResloader(scene.name);

                _allSceneUnits.Remove(scene);
                onSceneUnLoaded?.Invoke(sceneUnit);
            }
            else
            {
                Debug.LogWarning("SceneUnload not find scene:" + scene);
            }

        }

        protected class GSceneLoadProgress : IReusableClass
        {

            public uint MaxStore => 10;

            public float totalProgress { get; set; }

            public void OnReset()
            {
                totalProgress = 0;
            }
        }

        protected async UniTask LoadSceneAsyncInternal(string sceneName, LoadSceneMode mode, ISceneLoaderProcess process, bool loadByAb)
        {

            process.ShowSceneLoadingWindow();

            var progress = ClassPool.Get<GSceneLoadProgress>();

            //所有的加载任务
            List<UniTask> taskList = ListPool<UniTask>.Get();
            //unity场景的加载
            taskList.Add(UniTask.Defer(() => LoadNextScene(sceneName, mode, process, loadByAb, progress)));

            //预加载
            var preloadFuns = process.GetAsyncPreloads();
            if (preloadFuns != null)
            {
                taskList.AddRange(preloadFuns);
            }
            //除去加载纯scene，剩余的每个所占的比例
            var asyncUnit = (1 - SceneLoaderRatio) / (taskList.Count);
            for (int i = 0; i < taskList.Count; i++)
            {
                progress.totalProgress += asyncUnit;
                process.OnAsyncLoadProgress(progress.totalProgress);
                await taskList[i];
            }

            //返回对象池
            ListPool<UniTask>.Put(taskList);
            ClassPool.Put<GSceneLoadProgress>(progress);

            process.CloseSceneLoadingWindow();

        }

        protected async UniTask LoadNextScene(string sceneName, LoadSceneMode mode, ISceneLoaderProcess process, bool loadByAb, GSceneLoadProgress progress)
        {
            AsyncOperation _sceneAsync;
            if (loadByAb)
            {
                var sceneResloader = AddSceneWithResloader(sceneName);
#if UNITY_EDITOR
                if (!ResManager.EditorBundleMode)
                {
                    string scenePath = process.GetABScenePath(sceneName);
                    scenePath = "Assets/" + Path.ChangeExtension(scenePath, ".unity");
                    _sceneAsync = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(mode));
                }
                else
#endif
                {
                    string sceneABPath = process.GetABScenePath(sceneName);
                    await sceneResloader.LoadAssetBundleAwait(sceneABPath);
                    _sceneAsync = SceneManager.LoadSceneAsync(sceneName, mode);
                }
            }
            else
            {
                AddSceneOnly(sceneName);
                _sceneAsync = SceneManager.LoadSceneAsync(sceneName, mode);
            }

            _sceneAsync.allowSceneActivation = false;

            //计算加载进度
            float lastProgress = 0;
            while (!_sceneAsync.isDone)
            {
                while (_sceneAsync.progress < 0.9f)
                {
                    float curProgess = _sceneAsync.progress;
                    if (curProgess > lastProgress)
                    {
                        float grow = SceneLoaderRatio * (curProgess - lastProgress);
                        progress.totalProgress += grow;
                        process.OnAsyncLoadProgress(progress.totalProgress);
                    }
                    lastProgress = curProgess;
                    await UniTask.Yield();
                }
                _sceneAsync.allowSceneActivation = true;
                await UniTask.Yield();
            }

            progress.totalProgress += (1f - lastProgress) * SceneLoaderRatio;
            process.OnAsyncLoadProgress(progress.totalProgress);

            await UniTask.Yield();
        }

        #endregion

        #region public interface

        public bool ContainScene(string sceneName)
        {
            return _sceneNameCount.TryGetValue(sceneName, out var count) && count > 0;
        }

        /// <summary>
        /// 加载Build In场景-同步的方式
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="process"></param>
        public void LoadBuildinScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, ISceneLoaderProcess process = null)
        {
            process = process ?? defaultLoaderProcess;
            if (mode == LoadSceneMode.Additive)
            {
                AddSceneOnly(sceneName);
                SceneManager.LoadScene(sceneName, mode);
                return;
            }
            if (!process.isSingleForceLoadSame && ContainScene(sceneName))
            {
                return;
            }
            AddSceneOnly(sceneName);
            SceneManager.LoadScene(sceneName, mode);
        }

        /// <summary>
        /// 加载Build In场景-异步的方式
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public async UniTask LoadBuildinSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, ISceneLoaderProcess process = null)
        {
            process = process ?? defaultLoaderProcess;
            if (mode == LoadSceneMode.Additive)
            {
                await LoadSceneAsyncInternal(sceneName, mode, process, false);
                return;
            }
            if (!process.isSingleForceLoadSame && ContainScene(sceneName))
            {
                return;
            }
            await LoadSceneAsyncInternal(sceneName, mode, process, false);
        }

        /// <summary>
        /// 加载ab包的场景-异步的方式
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="process"></param>
        public void LoadAbScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, ISceneLoaderProcess process = null)
        {
            process = process ?? defaultLoaderProcess;
            var sceneResloader = AddSceneWithResloader(sceneName);
#if UNITY_EDITOR
            if (!ResManager.EditorBundleMode)
            {
                string scenePath = process.GetABScenePath(sceneName);
                scenePath = "Assets/" + Path.ChangeExtension(scenePath, ".unity");
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(mode));
            }
            else
#endif
            {
                string sceneABPath = process.GetABScenePath(sceneName);
                sceneResloader.LoadAssetBundle(sceneABPath);
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// 加载ab包的场景-异步的方式
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public async UniTask LoadAbSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, ISceneLoaderProcess process = null)
        {
            process = process ?? defaultLoaderProcess;
            if (mode == LoadSceneMode.Additive)
            {
                await LoadSceneAsyncInternal(sceneName, mode, process, true);
                return;
            }
            if (!process.isSingleForceLoadSame && ContainScene(sceneName))
            {
                return;
            }
            await LoadSceneAsyncInternal(sceneName, mode, process, true);
        }


        /// <summary>
        /// 卸载场景-异步方式
        /// </summary>
        /// <param name="sceneName"></param>
        public AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>
        /// 卸载场景-同步方式
        /// </summary>
        /// <param name="sceneName"></param>
        [Obsolete("Use GSceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details.")]
        public bool UnloadScene(string sceneName)
        {
            return SceneManager.UnloadScene(sceneName);
        }
        #endregion


    }

}

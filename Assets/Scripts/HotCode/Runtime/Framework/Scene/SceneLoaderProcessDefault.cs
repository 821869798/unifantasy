using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace HotCode.Framework
{
    public class SceneLoaderProcessDefault : ISceneLoaderProcess
    {
        public bool isSingleForceLoadSame { get; set; } = false;

        public virtual string GetABScenePath(string sceneName)
        {
            return PathConstant.GetABScenePath(sceneName);
        }

        public virtual void OnAsyncLoadProgress(float ratio)
        {

        }

        public virtual void ShowSceneLoadingWindow()
        {

        }

        public virtual void CloseSceneLoadingWindow()
        {

        }

        protected List<UniTask> _asyncPreloads;

        public void AddAsyncPreload(Func<UniTask> preloadFunc)
        {
            if (_asyncPreloads == null)
            {
                _asyncPreloads = new List<UniTask>();
            }
            //延迟到调用await的时候才会执行
            _asyncPreloads.Add(UniTask.Defer(preloadFunc));
        }

        public List<UniTask> GetAsyncPreloads()
        {
            return _asyncPreloads;
        }

        public void ClearAsyncPreloads()
        {
            if (_asyncPreloads != null)
            {
                _asyncPreloads.Clear();
            }
        }


    }
}

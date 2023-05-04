using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;


namespace HotCode.Framework
{
    public interface ISceneLoaderProcess
    {
        /// <summary>
        /// 是否强制重新加载已经存在的场景,只针对Single加载模式
        /// </summary>
        bool isSingleForceLoadSame { get; }

        /// <summary>
        /// 获取AB包的场景资源完整加载路径
        /// </summary>
        /// <param name="sceneName"></param>
        string GetABScenePath(string sceneName);

        /// <summary>
        /// 收到异步加载的进度通知,是当前总进度
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        void OnAsyncLoadProgress(float ratio);

        /// <summary>
        /// 添加预加载函数：将插入在unity Scene load之后预加载
        /// </summary>
        /// <param name="preloadFunc"></param>
        void AddAsyncPreload(Func<UniTask> preloadFunc);

        /// <summary>
        /// 获取所有的异步预加载函数
        /// </summary>
        /// <returns></returns>
        List<UniTask> GetAsyncPreloads();

        /// <summary>
        /// 清除所有的异步预加载函数
        /// </summary>
        void ClearAsyncPreloads();

        /// <summary>
        /// 展示LoadingUI
        /// </summary>
        void ShowSceneLoadingWindow();

        /// <summary>
        /// 关闭LoadingUI
        /// </summary>
        void CloseSceneLoadingWindow();

    }
}

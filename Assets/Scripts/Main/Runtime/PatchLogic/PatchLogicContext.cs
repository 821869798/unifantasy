using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniFan;
using UnityEngine;

namespace Main.HotUpdate
{
    /// <summary>
    /// 热更逻辑的上下文可传递数据
    /// </summary>
    public class PatchLogicContext : IDisposable
    {
        /// <summary>
        /// 匹配到的远程版本信息
        /// </summary>
        public PatchRemoteVersion remoteVersion;

        /// <summary>
        /// 本地的app整包版本(Application.version)
        /// </summary>
        public Version localAppVersion;

        /// <summary>
        /// 本地的版本信息(在沙盒目录的文件)
        /// </summary>
        public GameVersionInfo localVersionInfo;

        /// <summary>
        /// 用户的沙盒目录的中的文件信息
        /// </summary>
        public PatchManifestData sandboxPatchManifest;

        /// <summary>
        /// 最新的热更的文件列表信息
        /// </summary>
        public PatchManifestData latestPatchManifest;

        /// <summary>
        /// 需要更新本地VersionInfo文件
        /// </summary>
        public bool needUpdateLocalVersionInfo;

        /// <summary>
        /// 已经存在的资源文件列表，用来对比哪些文件需要下载，key：路径
        /// </summary>
        public readonly Dictionary<string, PatchFileInfo> existResourceFiles = new Dictionary<string, PatchFileInfo>();

        /// <summary>
        /// 需要下载的文件列表
        /// </summary>
        public readonly List<PatchFileInfo> needDownloadFiles = new List<PatchFileInfo>();

        /// <summary>
        /// 热更控制器
        /// </summary>
        private PatchController patchController;

        public PatchLogicContext(PatchController patchController)
        {
            this.patchController = patchController;
            localAppVersion = new Version(Application.version);
        }

        public void Dispose()
        {

        }

        public UniTask RunPatchLogic<T>() where T : IPatchLogic
        {
            return patchController.RunPatchLogic<T>();
        }

        /// <summary>
        /// 获取资源服务器地址,临时用
        /// </summary>
        public string GetHostServerURL(string version)
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = "http://127.0.0.1";
            // 临时版本号
            var platformName = FilePathHelper.GetPlatformName();
            return $"{hostServerIP}/CDN/{platformName}/{version}";
        }

        public string GetPatchFileDownloadCachePath(string filePath)
        {
            return FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.DownloadCachePath + "/" + filePath.Replace('/', '-'));
        }

    }

    [Serializable]
    /// <summary>
    /// 远程的版本信息
    /// </summary>
    public class PatchRemoteVersion
    {
        /// <summary>
        /// 远程的app整包版本
        /// </summary>
        public Version appVersion;

        /// <summary>
        /// 远程的资源版本
        /// </summary>
        public Version resVersion;
    }
}

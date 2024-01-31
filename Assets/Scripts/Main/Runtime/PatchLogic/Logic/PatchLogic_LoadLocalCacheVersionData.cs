using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UniFan;
using UnityEngine;

namespace Main.HotUpdate
{
    /// <summary>
    /// 加载本地缓存信息
    /// </summary>
    internal class PatchLogic_LoadLocalCacheVersionData : IPatchLogic
    {
        public async UniTask Run(PatchLogicContext patchContext)
        {

            // 加载本地版本信息
            if (TryLoadSandboxVersionInfo(out var localVersionInfo))
            {
                patchContext.localVersionInfo = localVersionInfo;

                // 如果本地就已经是最新
                if (localVersionInfo.appVersionObject == patchContext.localAppVersion &&
                    localVersionInfo.resVersionObject == patchContext.remoteVersion.resVersion &&
                    PatchLogicUtility.TryLoadLocalPatchManifestFile(patchContext.localVersionInfo.resVersion, out var patchManifestData)
                    )
                {
                    // 本地就是最新的，无需去服务器请求
                    patchContext.latestPatchManifest = patchManifestData;
                    patchContext.sandboxPatchManifest = patchManifestData;
                }
                else
                {
                    // 本地不是最新的，需要在更新完，写入新的版本信息
                    patchContext.needUpdateLocalVersionInfo = true;
                }
            }
            else
            {
                // 本地没有version.txt,需要在更新完，写入新的版本信息
                patchContext.needUpdateLocalVersionInfo = true;
            }

            if (patchContext.latestPatchManifest != null)
            {
                // 本地的就是最新的直接使用本地的
                await patchContext.RunPatchLogic<PatchLogic_VerifyExistResourceFiles>();
            }
            else
            {
                // 需要去下载服务器的版本信息
                await patchContext.RunPatchLogic<PatchLogic_RequestPatchManifest>();
            }
        }

        public bool TryLoadSandboxVersionInfo(out GameVersionInfo versionInfo)
        {
            versionInfo = null;
            try
            {
                var versionFilePath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.VersionFileName);
                if (!File.Exists(versionFilePath))
                {
                    return false;
                }
                var content = File.ReadAllText(versionFilePath);
                versionInfo = JsonUtility.FromJson<GameVersionInfo>(content);
                if (versionInfo != null)
                {
                    versionInfo.InitRead();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(PatchLogic_LoadLocalCacheVersionData)}|{nameof(TryLoadSandboxVersionInfo)}] Exception: {e}");
                return false;
            }
        }


        public void Dispose()
        {

        }
    }
}

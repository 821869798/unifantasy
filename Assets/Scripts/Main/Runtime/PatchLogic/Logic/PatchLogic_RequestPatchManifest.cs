using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UniFan;
using UnityEngine;
using UnityEngine.Networking;

namespace Main.HotUpdate
{
    /// <summary>
    /// 请求远程的PatchManifest，用于之后生成下载文件列表
    /// </summary>
    internal class PatchLogic_RequestPatchManifest : IPatchLogic
    {
        public async UniTask Run(PatchLogicContext patchContext)
        {
            var result = false;
            while (!result)
            {
                try
                {
                    result = await RequestRemotePatchManifest(patchContext);
                }
                catch (UnityWebRequestException e)
                {
                    UnityEngine.Debug.LogWarning($"request patch manifest failed:{e}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"RequestRemotePatchManifest failed:{e}");
                    // todo 处理异常情况
                    return;
                }

                // TODO,测试代码，实际要提示用户失败
                await UniTask.Delay(3000);

            }

            await patchContext.RunPatchLogic<PatchLogic_VerifyExistResourceFiles>();
        }
        public async UniTask<bool> RequestRemotePatchManifest(PatchLogicContext patchContext)
        {

            var versionString = patchContext.remoteVersion.resVersion.ToString();
            var hashUrl = $"{patchContext.GetHostServerURL(versionString)}/{PatchLogicUtility.PatchManfistRootPath}/{PatchLogicUtility.GetPatchManfistHashFileName(versionString)}";

            string hashValue = string.Empty;
            // 首先请求PatchManifest的hash值
            using (var requestHash = UnityWebRequest.Get(hashUrl))
            {
                await requestHash.SendWebRequest();
                if (requestHash.result != UnityWebRequest.Result.Success || requestHash.responseCode != 200)
                {
                    // TODO 错误提示
                    UnityEngine.Debug.LogWarning($"request patch manifest hash value failed:{requestHash.error}");
                    return false;
                }
                hashValue = requestHash.downloadHandler.text;
                if (hashValue.Length != 32)
                {
                    return false;
                }
            }

            var fileUrl = $"{patchContext.GetHostServerURL(versionString)}/{PatchLogicUtility.PatchManfistRootPath}/{PatchLogicUtility.GetPatchManfistFileName(versionString)}";
            var cachePath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.DownloadCachePath + "/" + hashValue + ".bytes");
            UnityWebRequestFileDownload webRequestFileDownload = new UnityWebRequestFileDownload();
            var downloadResult = await webRequestFileDownload.DownloadFile(fileUrl, cachePath);

            if (downloadResult != UnityWebRequestFileDownload.FileDownloadResult.Success)
            {
                // TODO 错误提示
                UnityEngine.Debug.LogWarning($"Download patch manifest failed:{downloadResult}");
                return false;
            }

            if (!HashUtility.TryFileMD5(cachePath, out var currentHash))
            {
                UnityEngine.Debug.LogWarning($"Get patch manifest hash failed:{cachePath}");
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }
                return false;
            }

            if (currentHash != hashValue)
            {
                UnityEngine.Debug.LogWarning($"Patch manifest hash not match:{currentHash} != {hashValue}");
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }
                return false;
            }

            // 移动到目标目录去
            var destPath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.PatchManfistRootPath + "/" + PatchLogicUtility.GetPatchManfistFileName(versionString));
            // 判断目标文件是否存在
            if (File.Exists(destPath))
            {
                // 删除目标文件
                File.Delete(destPath);
            }
            File.Move(cachePath, destPath);

            if (!PatchLogicUtility.TryLoadLocalPatchManifestFile(versionString, out var patchManifestData))
            {
                return false;
            }

            patchContext.latestPatchManifest = patchManifestData;

            return true;
        }

        public void Dispose()
        {

        }


    }
}

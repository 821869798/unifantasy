using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UniFan;
using UnityEngine;
using UnityEngine.Networking;

namespace Main.HotUpdate
{
    /// <summary>
    /// 校验已经资源，
    /// </summary>
    internal class PatchLogic_VerifyExistResourceFiles : IPatchLogic
    {
        public async UniTask Run(PatchLogicContext patchContext)
        {
            patchContext.existResourceFiles.Clear();

            var buildinPatchManifest = await LoadBuildinPatchManifest(patchContext);
            if (buildinPatchManifest != null)
            {
                // 内部的文件当作一定存在
                foreach (var p in buildinPatchManifest.fileInfoList)
                {
                    patchContext.existResourceFiles[p.filePath] = p;
                }
            }

            var sandboxPatchManifest = patchContext.sandboxPatchManifest;

            if (sandboxPatchManifest == null && patchContext.localVersionInfo != null &&
                PatchLogicUtility.TryLoadLocalPatchManifestFile(patchContext.localVersionInfo.resVersion, out var patchManifestData)
                )
            {
                sandboxPatchManifest = patchManifestData;
            }

            if (sandboxPatchManifest != null)
            {
                // 用户沙盒目录中的热更信息列表，加入已存在的文件作为缓存
                foreach (var p in sandboxPatchManifest.fileInfoList)
                {
                    var fullPath = FilePathHelper.Instance.GetPersistentDataPath(p.filePath);
                    //if (!File.Exists(fullPath))
                    //{
                    //    continue;
                    //}

                    // 校验本地已存在文件，用是否存在和文件长度
                    FileInfo fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Exists)
                    {
                        // 这里校验了本地文件的大小，会影响手动push资源到手机上模拟热更测试的操作。可以加个开关之类的
                        if (fileInfo.Length == p.fileSize)
                        {
                            patchContext.existResourceFiles[p.filePath] = p;
                        }
                        else
                        {
                            patchContext.existResourceFiles.Remove(p.filePath);
                        }
                    }

                }
            }

            await patchContext.RunPatchLogic<PatchLogic_CreatePatchDownloader>();

        }

        /// <summary>
        /// 加载包内的文件做校验用
        /// </summary>
        /// <param name="patchContext"></param>
        /// <returns></returns>
        public async UniTask<PatchManifestData> LoadBuildinPatchManifest(PatchLogicContext patchContext)
        {
            try
            {
                var patchManifestFilePath = FilePathHelper.Instance.GetStreamingPathForWWW(PatchLogicUtility.PatchManfistRootPath + "/" + PatchLogicUtility.GetPatchManfistFileName(patchContext.localAppVersion.ToString()));

                using (UnityWebRequest request = UnityWebRequest.Get(patchManifestFilePath))
                {
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
                    {
                        Debug.LogWarning($"[{nameof(PatchLogic_VerifyExistResourceFiles)}|{nameof(LoadBuildinPatchManifest)}] Error: {request.error}");
                        return null;
                    }

                    using (MemoryStream ms = new MemoryStream(request.downloadHandler.data))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            var patchManifestData = new PatchManifestData();
                            patchManifestData.Read4Binary(br);
                            return patchManifestData;
                        }
                    }

                }
            }
            catch (UnityWebRequestException)
            {
                // 其实没啥关系，就是空包的时候内部没有PatchManifest文件
                Debug.LogWarning($"[{nameof(PatchLogic_VerifyExistResourceFiles)}|{nameof(LoadBuildinPatchManifest)}] build in config not found");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(PatchLogic_VerifyExistResourceFiles)}|{nameof(LoadBuildinPatchManifest)}] Exception: {e}");
            }

            return null;
        }

        public void Dispose()
        {

        }
    }
}

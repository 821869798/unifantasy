using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UniFan;
using UnityEngine;

namespace Main.HotUpdate
{
    // 下载完毕，开始移动文件
    internal class PatchLogic_AfterDownloadComplete : IPatchLogic
    {
        public async UniTask Run(PatchLogicContext patchContext)
        {
            while (true)
            {
                try
                {
                    // 移动文件到目标目录中去
                    foreach (var patchFile in patchContext.needDownloadFiles)
                    {
                        var cachePath = patchContext.GetPatchFileDownloadCachePath(patchFile.filePath);
                        var destPath = FilePathHelper.Instance.GetPersistentDataPath(patchFile.filePath);
                        var parentPath = Path.GetDirectoryName(destPath);
                        if (!Directory.Exists(parentPath))
                        {
                            Directory.CreateDirectory(parentPath);
                        }
                        File.Copy(cachePath, destPath, true);
                    }
                    // 成功就离开
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{nameof(PatchLogic_AfterDownloadComplete)}|{nameof(Run)}] move patch file  Exception: {e}");
                    // TODO, 比较验证，弹框提示失败,现在临时等待5秒继续尝试
                    await UniTask.Delay(5000);
                }
            }

            try
            {
                if (patchContext.needUpdateLocalVersionInfo)
                {
                    // 写入VersionInfo文件
                    GameVersionInfo gameVersionInfo = new GameVersionInfo()
                    {
                        appVersionObject = patchContext.remoteVersion.appVersion,
                        resVersionObject = patchContext.remoteVersion.resVersion,
                    };
                    gameVersionInfo.InitWrite();

                    var versionFilePath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.VersionFileName);
                    var json = JsonUtility.ToJson(gameVersionInfo);
                    File.WriteAllText(versionFilePath, json);

                    var patchPath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.PatchManfistRootPath);
                    var exclude = new System.Collections.Generic.HashSet<string>() { PatchLogicUtility.GetPatchManfistFileName(patchContext.remoteVersion.resVersion.ToString()) };
                    FilePathHelper.DeleteDir(patchPath, false, exclude);
                }
            }
            catch (Exception e)
            {
                // TODO，比较严重，没法写入版本文件
                Debug.LogError($"[{nameof(PatchLogic_AfterDownloadComplete)}|{nameof(Run)}] write version file Exception: {e}");
            }

            try
            {
                // 以及删除不需要的文件
                foreach (var patchFile in patchContext.existResourceFiles)
                {
                    var destPath = FilePathHelper.Instance.GetPersistentDataPath(patchFile.Value.filePath);
                    // 要判断文件是否存在，因为可能是包内资源，不是用户沙盒目录中的
                    if (File.Exists(destPath))
                    {
                        File.Delete(destPath);
                    }
                }

                // 更新完成，删除cache目录
                var cachePathRoot = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.DownloadCachePath);
                if (Directory.Exists(cachePathRoot))
                {
                    Directory.Delete(cachePathRoot, true);
                }
            }
            catch (Exception e)
            {
                // TODO,删除不掉，但是不影响继续玩
                Debug.LogWarning($"[{nameof(PatchLogic_AfterDownloadComplete)}|{nameof(Run)}] delete patch file  Exception: {e}");
            }

            await patchContext.RunPatchLogic<PatchLogic_FinishPatchDone>();
        }

        public void Dispose()
        {

        }
    }
}

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Main.HotUpdate
{
    internal class PatchLogic_DownloadPatchFiles : IPatchLogic
    {


        public async UniTask Run(PatchLogicContext patchContext)
        {
            List<IDownloadFile> needDownloadFiles = new List<IDownloadFile>(patchContext.needDownloadFiles);
            using (var downloader = new PatchParallelDownloader(patchContext, needDownloadFiles, 5))
            {
                // 判断磁盘空间够不够
                // PlatformUtility.GetFreeDiskSpace();

                while (true)
                {
                    var time = Time.realtimeSinceStartup;
                    var result = await downloader.StartDownloadAsync();
                    if (result == PatchParallelDownloader.PatchDownloadResult.Success)
                    {
                        Debug.Log($"[{nameof(PatchLogic_DownloadPatchFiles)}] Download files count:{patchContext.needDownloadFiles.Count} time cost:{(Time.realtimeSinceStartup - time).ToString("F3")} ");
                        await patchContext.RunPatchLogic<PatchLogic_AfterDownloadComplete>();
                        return;
                    }
                    // TODO 下载出错，弹窗重试，StartDownloadAsync支持重复调用
                    Debug.LogWarning($"download patch files failed:{result} error file:{downloader.failedDownloadFile?.Name ?? string.Empty}");
                    await UniTask.Delay(3000);
                }
            }
        }

        public void Dispose()
        {

        }

    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Main.HotUpdate
{
    internal class PatchLogic_DownloadPatchFiles : IPatchLogic
    {


        public async UniTask Run(PatchLogicContext patchContext)
        {
            using (var downloader = new PatchDownloadParallel(patchContext, patchContext.needDownloadFiles, 5))
            {
                // 判断磁盘空间够不够
                // PlatformUtility.GetFreeDiskSpace();

                while (true)
                {
                    var time = Time.realtimeSinceStartup;
                    var result = await downloader.StartDownloadAsync();
                    if (result == PatchDownloadParallel.PatchDownloadResult.Success)
                    {
                        Debug.Log($"[{nameof(PatchLogic_DownloadPatchFiles)}] Download files count:{patchContext.needDownloadFiles.Count} time cost:{(Time.realtimeSinceStartup - time).ToString("F3")} ");
                        await patchContext.RunPatchLogic<PatchLogic_AfterDownloadComplete>();
                        return;
                    }
                    // TODO 下载出错，弹窗重试，StartDownloadAsync支持重复调用
                    Debug.LogWarning($"download patch files failed:{result} error file:{downloader.failedDownloadFile?.filePath ?? string.Empty}");
                    await UniTask.Delay(3000);
                }
            }
        }

        public void Dispose()
        {

        }

    }
}

using Cysharp.Threading.Tasks;

namespace Main.HotUpdate
{
    /// <summary>
    /// 创建下载列表
    /// </summary>
    internal class PatchLogic_CreatePatchDownloader : IPatchLogic
    {
        public UniTask Run(PatchLogicContext patchContext)
        {
            patchContext.needDownloadFiles.Clear();
            // existResourceFiles 最后留下的就是需要删除的文件
            // 但是业务逻辑还是不能依赖File.Exist，应该通过配置来判断

            foreach (var fileInfo in patchContext.latestPatchManifest.fileInfoList)
            {
                if (patchContext.existResourceFiles.TryGetValue(fileInfo.filePath, out var existFile))
                {
                    patchContext.existResourceFiles.Remove(fileInfo.filePath);

                    if (existFile.fileMd5 != fileInfo.fileMd5)
                    {
                        // 已存在的文件，且md5一致，不需要下载
                        patchContext.needDownloadFiles.Add(fileInfo);
                    }
                }
                else
                {
                    // 不存在的文件，需要下载
                    patchContext.needDownloadFiles.Add(fileInfo);
                }
            }

            UnityEngine.Debug.Log($"[{nameof(PatchLogic_CreatePatchDownloader)}] create downloader count:{patchContext.needDownloadFiles.Count.ToString()}");

            return patchContext.RunPatchLogic<PatchLogic_DownloadPatchFiles>();

        }

        public void Dispose()
        {

        }


    }
}

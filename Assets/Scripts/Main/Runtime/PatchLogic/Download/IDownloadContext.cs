namespace Main.HotUpdate
{
    public interface IDownloadContext
    {
        /// <summary>
        /// 获取文件的下载链接
        /// </summary>
        /// <param name="downloadFile"></param>
        /// <returns></returns>
        public string GetDownloadUrl(IDownloadFile downloadFile);

        /// <summary>
        /// 获取文件的本地存储路径
        /// </summary>
        /// <param name="downloadFile"></param>
        /// <returns></returns>
        public string GetTargetFilePath(IDownloadFile downloadFile);

        /// <summary>
        /// 下载一个文件完成时
        /// </summary>
        /// <param name="downloadFile"></param>
        /// <returns></returns>
        public void OnOneFileDownloaded(IDownloadFile downloadFile);
    }
}
namespace UniFan
{
    public interface IFileDownloadHandle
    {
        void OnDownloadBytesIncrease(long downloadBytes, float progress);

        void OnDownloadFailed(long existFileLength);
    }
}
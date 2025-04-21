using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UniFan;
using UnityEngine;

namespace Main.HotUpdate
{
    /// <summary>
    /// 并行文件下载器
    /// </summary>
    public class PatchParallelDownloader : IDisposable, IFileDownloadHandle
    {
        private List<IDownloadFile> _needDownloadFiles;
        private IDownloadContext _downloadContext;

        /// <summary>
        /// 已经下载成功的大小
        /// </summary>
        private long _lastDownloadBytes = 0;

        /// <summary>
        /// 下载中的大小
        /// </summary>
        private long _downloadingBytes = 0;

        private int _lastDownloadCount = 0;

        private readonly int _downloadingMaxNumber;
        private readonly int _failedTryAgain;
        private float _timeout;

        /// <summary>
        /// 统计的下载文件总数量
        /// </summary>
        public int TotalDownloadCount { private set; get; }

        /// <summary>
        /// 统计的下载文件的总大小
        /// </summary>
        public long TotalDownloadBytes { private set; get; }

        /// <summary>
        /// 当前已经完成的下载总数量
        /// </summary>
        public int CurrentDownloadCount
        {
            get { return _lastDownloadCount; }
        }

        public bool IsDone => CurrentDownloadCount >= TotalDownloadCount;

        /// <summary>
        /// 当前已经完成的下载总大小
        /// </summary>
        public long CurrentDownloadBytes
        {
            get { return _lastDownloadBytes + _downloadingBytes; }
        }


        private readonly Queue<IDownloadFile> _waitDownloadFiles;
        private readonly HashSet<IDownloadFile> _downloadingFiles = new HashSet<IDownloadFile>();

        /// <summary>
        ///  具体失败的文件
        /// </summary>
        private IDownloadFile _failedDownloadFile;
        public IDownloadFile failedDownloadFile => _failedDownloadFile;
        private UniTaskCompletionSource<PatchDownloadResult> _taskCompleteSource;
        private CancellationTokenSource _cts;

        public enum PatchDownloadResult
        {
            Success = 0,
            Failed = 1,
        }


        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="patchLogicContext"></param>
        /// <param name="downloadingMaxNumber">同时下载文件的数量</param>
        /// <param name="failedTryAgain">单个文件失败尝试次数</param>
        /// <param name="timeout">单个文件的超时时间</param>
        public PatchParallelDownloader(IDownloadContext downloadContext, List<IDownloadFile> downloadFileList, int downloadingMaxNumber = 5, int failedTryAgain = 1, float timeout = 5f)
        {
            this._downloadContext = downloadContext;
            this._needDownloadFiles = downloadFileList;
            this._downloadingMaxNumber = downloadingMaxNumber;
            this._failedTryAgain = failedTryAgain;
            this._timeout = timeout;

            _waitDownloadFiles = new Queue<IDownloadFile>(downloadFileList.Count);
            for (int i = 0; i < downloadFileList.Count; i++)
            {
                _waitDownloadFiles.Enqueue(downloadFileList[i]);
            }

            CalculatDownloaderInfo();
        }

        private void CalculatDownloaderInfo()
        {
            _lastDownloadBytes = 0;
            _lastDownloadCount = 0;
            TotalDownloadCount = _needDownloadFiles.Count;
            TotalDownloadBytes = 0;
            _downloadingFiles.Clear();
            foreach (var downloader in _needDownloadFiles)
            {
                TotalDownloadBytes += downloader.FileSize;

            }
        }

        /// <summary>
        /// 新的下载，或者失败之后弹框点击重试的继续下载
        /// </summary>
        public UniTask<PatchDownloadResult> StartDownloadAsync()
        {
            if (_cts != null)
            {
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource();
            _taskCompleteSource = new UniTaskCompletionSource<PatchDownloadResult>();

            _failedDownloadFile = null;
            _downloadingBytes = 0;

            // 如果上次有文件下载中，优先下载这些文件
            if (_downloadingFiles.Count > 0)
            {
                var list = new List<IDownloadFile>();
                if (_failedDownloadFile != null)
                {
                    list.Add(_failedDownloadFile);
                    _downloadingFiles.Remove(_failedDownloadFile);
                }
                list.AddRange(_downloadingFiles);
                _downloadingFiles.Clear();
                foreach (var downloader in list)
                {
                    DownloadOneFile(downloader).Forget();
                }
            }

            _failedDownloadFile = null;

            DownloadFiles();

            return _taskCompleteSource.Task;
        }

        private void DownloadFiles()
        {
            if (IsDone)
            {
                if (_taskCompleteSource != null)
                {
                    _taskCompleteSource.TrySetResult(PatchDownloadResult.Success);
                }
                return;
            }


            while (_downloadingFiles.Count < this._downloadingMaxNumber && _waitDownloadFiles.Count > 0)
            {
                var patchFileInfo = _waitDownloadFiles.Dequeue();
                DownloadOneFile(patchFileInfo).Forget();
            }
        }

        async UniTask DownloadOneFile(IDownloadFile patchFileInfo)
        {

            _downloadingFiles.Add(patchFileInfo);
            try
            {
                int failedCount = -1;
                do
                {
                    var downloadUrl = this._downloadContext.GetDownloadUrl(patchFileInfo);
                    var targetFilePath = this._downloadContext.GetTargetFilePath(patchFileInfo);

                    // 下载文件
                    var result = await UnityWebRequestFileDownload.DownloadFileVerifyMD5(
                        downloadUrl,
                        targetFilePath,
                        patchFileInfo.Hash,
                        patchFileInfo.FileSize,
                        _cts.Token,
                        _timeout,
                        this
                        );

                    if (result == UnityWebRequestFileDownload.FileDownloadResult.Success)
                    {
                        // 成功，继续下载
                        DownloadOneFileSuccess(patchFileInfo);
                        return;
                    }

                    // 失败次数加1
                    failedCount++;

                } while (failedCount < this._failedTryAgain);


            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{nameof(PatchParallelDownloader)}|{nameof(DownloadOneFile)}] Exception: {e}");
            }

            _failedDownloadFile = patchFileInfo;

            // 失败了次数过多
            _cts.Cancel();
            if (_taskCompleteSource != null)
            {
                _taskCompleteSource.TrySetResult(PatchDownloadResult.Failed);
            }
        }

        public void OnDownloadBytesIncrease(long downloadBytes, float progress)
        {
            _downloadingBytes += downloadBytes;
        }

        public void OnDownloadFailed(long existFileLength)
        {
            _downloadingBytes -= existFileLength;
        }

        /// <summary>
        /// 下载成功一个文件
        /// </summary>
        /// <param name="file"></param>
        private void DownloadOneFileSuccess(IDownloadFile file)
        {
            _lastDownloadCount++;
            _lastDownloadBytes += file.FileSize;
            _downloadingBytes -= file.FileSize;
            _downloadingFiles.Remove(file);

            _downloadContext.OnOneFileDownloaded(file);

            DownloadFiles();
        }

        public void Dispose()
        {
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }

        }
    }
}

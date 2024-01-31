using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UniFan;
using UnityEngine;

namespace Main.HotUpdate
{
    /// <summary>
    /// 并行文件下载器
    /// </summary>
    public class PatchDownloadParallel : IDisposable
    {

        PatchLogicContext _patchLogicContext;
        string _versionString;
        private long _lastDownloadBytes = 0;
        private int _lastDownloadCount = 0;

        private readonly int _downloadingMaxNumber;
        private readonly int _failedTryAgain;
        private float _timeout;

        /// <summary>
        /// 统计的下载文件总数量
        /// </summary>
        public int TotalDownloadCount
        { private set; get; }

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

        /// <summary>
        /// 当前已经完成的下载总大小
        /// </summary>
        public long CurrentDownloadBytes
        {
            get { return _lastDownloadBytes; }
        }


        private readonly Queue<PatchFileInfo> _waitDownloadFiles;
        private readonly HashSet<PatchFileInfo> _downloadingFiles = new HashSet<PatchFileInfo>();

        /// <summary>
        ///  具体失败的文件
        /// </summary>
        private PatchFileInfo _failedDownloadFile;
        public PatchFileInfo failedDownloadFile => _failedDownloadFile;
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
        internal PatchDownloadParallel(PatchLogicContext patchLogicContext, List<PatchFileInfo> downloadList, int downloadingMaxNumber = 5, int failedTryAgain = 1, float timeout = 5f)
        {
            this._patchLogicContext = patchLogicContext;
            this._downloadingMaxNumber = downloadingMaxNumber;
            this._failedTryAgain = failedTryAgain;
            this._timeout = timeout;
            this._versionString = patchLogicContext.remoteVersion.resVersion.ToString();

            _waitDownloadFiles = new Queue<PatchFileInfo>(downloadList.Count);
            for (int i = 0; i < downloadList.Count; i++)
            {
                _waitDownloadFiles.Enqueue(downloadList[i]);
            }

            CalculatDownloaderInfo();
        }

        private void CalculatDownloaderInfo()
        {
            _lastDownloadBytes = 0;
            _lastDownloadCount = 0;
            TotalDownloadCount = _patchLogicContext.needDownloadFiles.Count;
            TotalDownloadBytes = 0;
            _downloadingFiles.Clear();
            foreach (var downloader in _patchLogicContext.needDownloadFiles)
            {
                TotalDownloadBytes += downloader.fileSize;

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

            // 如果上次有文件下载中，优先下载这些文件
            if (_downloadingFiles.Count > 0)
            {
                var list = new List<PatchFileInfo>();
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
            if (CurrentDownloadCount >= TotalDownloadCount)
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

        async UniTask DownloadOneFile(PatchFileInfo patchFileInfo)
        {

            _downloadingFiles.Add(patchFileInfo);
            try
            {
                int failedCount = 0;
                do
                {
                    WebRequestFileDownload webRequestFileDownload = new WebRequestFileDownload(_timeout);
                    var url = _patchLogicContext.GetHostServerURL(this._versionString) + "/" + patchFileInfo.filePath;

                    var cachePath = this._patchLogicContext.GetPatchFileDownloadCachePath(patchFileInfo.filePath);

                    var result = await webRequestFileDownload.DownloadFile(url, cachePath, (ulong)patchFileInfo.fileSize, _cts.Token);

                    if (result != WebRequestFileDownload.FileDownloadResult.Success)
                    {
                        failedCount++;
                    }
                    else
                    {
                        // 判断md5码
                        if (HashUtility.TryFileMD5(cachePath, out var md5Value) && md5Value == patchFileInfo.fileMd5)
                        {
                            // 下载成功
                            _lastDownloadCount++;
                            _lastDownloadBytes += patchFileInfo.fileSize;
                            _downloadingFiles.Remove(patchFileInfo);

                            // 调用继续下载
                            DownloadFiles();

                            return;
                        }

                        // md5校验失败
                        failedCount++;
                        if (File.Exists(cachePath))
                        {
                            File.Delete(cachePath);
                        }

                    }

                } while (failedCount > this._failedTryAgain);


            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{nameof(PatchDownloadParallel)}|{nameof(DownloadOneFile)}] Exception: {e}");
            }

            _failedDownloadFile = patchFileInfo;

            // 失败了次数过多
            _cts.Cancel();
            if (_taskCompleteSource != null)
            {
                _taskCompleteSource.TrySetResult(PatchDownloadResult.Failed);
            }
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

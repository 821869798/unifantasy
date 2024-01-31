using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace UniFan
{
    /// <summary>
    /// 支持断点续传的文件下载器
    /// 适用于热更下载
    /// 不适用于边下边玩，如果需要边下边玩，建议使用Task.Run + C# Http。因为Unity的会使用主线程来IO。
    /// </summary>
    public class WebRequestFileDownload
    {
        /// <summary>
        /// 完成的结果状态
        /// </summary>
        public enum FileDownloadResult
        {
            Success = 0,
            HeaderError = 1,
            DownloadError = 2,
            DownloadTimeout = 3,
            Exception = 4,
            Interrupt = 5,
        }

        /// <summary>
        /// 进度， (fileExistSize + fileDownloadedSize) / fileTotalSize
        /// </summary>
        public float progress { get; private set; }

        /// <summary>
        /// 下载之前已经存在的文件大小
        /// </summary>
        public ulong fileExistSize { get; private set; }

        /// <summary>
        /// 这次下载的字节大小
        /// </summary>
        public ulong fileDownloadedSize { get; private set; }

        /// <summary>
        /// 文件总的字节大小
        /// </summary>
        public ulong fileTotalSize { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get; private set; }

        /// <summary>
        /// 进度更新事件
        /// </summary>
        public Action<WebRequestFileDownload> onProcessUpdate;

        /// <summary>
        ///  超时时间
        /// </summary>
        private float _timeout;

        public WebRequestFileDownload(float timeout = 5f)
        {
            this._timeout = timeout;
        }

        /// <summary>
        /// 下载文件，不知道文件大小的情况下
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileSavePath"></param>
        /// <returns></returns>
        public async UniTask<FileDownloadResult> DownloadFile(string url, string fileSavePath, CancellationToken cancellationToken = default)
        {
            progress = 0;
            error = null;
            if (cancellationToken.IsCancellationRequested)
            {
                return FileDownloadResult.Interrupt;
            }
            try
            {
                using (UnityWebRequest headRequest = UnityWebRequest.Head(url))
                {
                    headRequest.timeout = Mathf.CeilToInt(this._timeout);
                    //开始与远程服务器通信。
                    await headRequest.SendWebRequest().WithCancellation(cancellationToken);
                    if (headRequest.result != UnityWebRequest.Result.Success || headRequest.responseCode != 200)
                    {
                        error = headRequest.error;
                        return FileDownloadResult.HeaderError;
                    }

                    ulong totalLength = ulong.Parse(headRequest.GetResponseHeader("Content-Length"));

                    return await DownloadFile(url, fileSavePath, totalLength, cancellationToken);
                }
            }
            catch (UnityWebRequestException)
            {
                return FileDownloadResult.DownloadError;
            }
            catch (OperationCanceledException)
            {
                // UniTask 取消异常
                return FileDownloadResult.Interrupt;
            }
            catch (Exception e)
            {
                // 其他异常
                error = e.ToString();
                return FileDownloadResult.Exception;
            }
        }

        /// <summary>
        /// 下载文件，已知文件大小的情况下
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileSavePath"></param>
        /// <param name="downFileSize"></param>
        /// <returns></returns>
        public async UniTask<FileDownloadResult> DownloadFile(string url, string fileSavePath, ulong fileTotalSize, CancellationToken cancellationToken = default)
        {
            error = null;
            progress = 0;
            fileExistSize = 0;
            if (cancellationToken.IsCancellationRequested)
            {
                return FileDownloadResult.Interrupt;
            }
            this.fileTotalSize = fileTotalSize;

            try
            {
                if (File.Exists(fileSavePath)) //检测资源是否存在
                {
                    var fileInfo = new FileInfo(fileSavePath);
                    fileExistSize = (ulong)fileInfo.Length;
                }
                if (fileExistSize >= fileTotalSize)
                {
                    return FileDownloadResult.Success;
                }
                using (var request = UnityWebRequest.Get(url))
                {
                    request.downloadHandler = new DownloadHandlerFile(fileSavePath);
                    if (fileExistSize > 0) //检测资源是否存在
                    {
                        //从该位置开始断点续传
                        request.SetRequestHeader("Range", "bytes=" + fileExistSize + "-");
                        // 更新进度
                        progress = (float)fileExistSize / fileTotalSize;
                        onProcessUpdate?.Invoke(this);
                    }
                    else
                    {
                        onProcessUpdate?.Invoke(this);
                    }


                    var timeoutTimer = Time.realtimeSinceStartup + _timeout;

                    _ = request.SendWebRequest();
                    while (!request.isDone)
                    {
                        // 等待一帧
                        await UniTask.Yield(cancellationToken);

                        var downloadedSize = request.downloadedBytes;
                        var realtimeSinceStartup = Time.realtimeSinceStartup;
                        if (downloadedSize > fileDownloadedSize)
                        {
                            fileDownloadedSize = downloadedSize;
                            // 只要收到了数据，就重置超时时间
                            timeoutTimer = realtimeSinceStartup + _timeout;
                        }
                        else if (realtimeSinceStartup > timeoutTimer)
                        {
                            // 超时
                            request.Abort();
                            return FileDownloadResult.DownloadTimeout;
                        }
                        progress = (float)(fileDownloadedSize + fileExistSize) / fileTotalSize;
                        onProcessUpdate?.Invoke(this);
                    }

                    return FileDownloadResult.Success;
                }
            }
            catch (UnityWebRequestException)
            {
                return FileDownloadResult.DownloadError;
            }
            catch (OperationCanceledException)
            {
                // UniTask 取消异常
                return FileDownloadResult.Interrupt;
            }
            catch (Exception e)
            {
                // 其他异常
                error = e.ToString();
                return FileDownloadResult.Exception;
            }

        }

    }
}
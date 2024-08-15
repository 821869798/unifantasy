using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Security.Cryptography;
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
    public class UnityWebRequestFileDownload
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
            VerifyError = 6,
        }

        /// <summary>
        /// 进度， (fileExistSize + fileDownloadedSize) / fileTotalSize
        /// </summary>
        public float progress
        {
            get
            {
                if (fileTotalSize == 0)
                {
                    return 0;
                }
                return (fileExistSize + fileDownloadedSize) / (float)fileTotalSize;
            }
        }

        /// <summary>
        /// 下载之前已经存在的文件大小
        /// </summary>
        public long fileExistSize { get; private set; }

        /// <summary>
        /// 这次下载的字节大小
        /// </summary>
        public long fileDownloadedSize { get; private set; }

        /// <summary>
        /// 文件总的字节大小
        /// </summary>
        public long fileTotalSize { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get; private set; }

        /// <summary>
        /// 进度更新事件
        /// </summary>
        private IFileDownloadHandle _fileDownloadHandle;

        /// <summary>
        ///  超时时间
        /// </summary>
        private float _timeout;

        private const float DefaultTimeout = 5f;

        public UnityWebRequestFileDownload(float timeout = DefaultTimeout, IFileDownloadHandle handle = null)
        {
            this._timeout = timeout;
            this._fileDownloadHandle = handle;
        }

        /// <summary>
        /// 下载文件，不知道文件大小的情况下
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileSavePath"></param>
        /// <returns></returns>
        public async UniTask<FileDownloadResult> DownloadFile(string url, string fileSavePath, CancellationToken cancellationToken = default)
        {
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

                    long totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));

                    return await DownloadFile(url, fileSavePath, totalLength, cancellationToken);
                }
            }
            catch (UnityWebRequestException e)
            {
                error = e.ToString();
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
        public async UniTask<FileDownloadResult> DownloadFile(string url, string fileSavePath, long fileTotalSize, CancellationToken cancellationToken = default)
        {
            error = null;
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
                    fileExistSize = fileInfo.Length;
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
                        _fileDownloadHandle?.OnDownloadBytesIncrease(fileExistSize, progress);
                    }


                    var timeoutTimer = Time.realtimeSinceStartup + _timeout;

                    _ = request.SendWebRequest();
                    while (!request.isDone)
                    {
                        // 等待一帧
                        await UniTask.Yield(cancellationToken);

                        var downloadedSize = (long)request.downloadedBytes;
                        var realtimeSinceStartup = Time.realtimeSinceStartup;
                        if (downloadedSize > fileDownloadedSize)
                        {
                            var increaseSize = downloadedSize - fileDownloadedSize;
                            fileDownloadedSize = downloadedSize;
                            _fileDownloadHandle?.OnDownloadBytesIncrease(increaseSize, progress);
                            // 只要收到了数据，就重置超时时间
                            timeoutTimer = realtimeSinceStartup + _timeout;
                        }
                        else if (realtimeSinceStartup > timeoutTimer)
                        {
                            // 超时
                            request.Abort();
                            return FileDownloadResult.DownloadTimeout;
                        }
                    }

                    return FileDownloadResult.Success;
                }
            }
            catch (UnityWebRequestException e)
            {
                InvokeDownloadHandleFailed();
                error = e.ToString();
                return FileDownloadResult.DownloadError;
            }
            catch (OperationCanceledException)
            {
                // UniTask 取消异常
                InvokeDownloadHandleFailed();
                return FileDownloadResult.Interrupt;
            }
            catch (Exception e)
            {
                // 其他异常
                InvokeDownloadHandleFailed();
                error = e.ToString();
                return FileDownloadResult.Exception;
            }

        }

        private void InvokeDownloadHandleFailed()
        {
            if (_fileDownloadHandle != null)
            {
                _fileDownloadHandle.OnDownloadFailed(fileExistSize + fileDownloadedSize);
            }
        }


        public static async UniTask<UnityWebRequestFileDownload.FileDownloadResult> DownloadFileVerifyMD5(string downloadUrl, string targetFilePath, string md5, long fileSize = 0, CancellationToken cancellationToken = default, float timeout = DefaultTimeout, IFileDownloadHandle handle = null)
        {
            string downloadTempPath = targetFilePath + ".download";
            if (Directory.Exists(downloadTempPath))
            {
                Directory.CreateDirectory(downloadTempPath);
            }
            string downloadTempFilePath = Path.Combine(downloadTempPath, md5 + ".temp");


            var downloader = new UnityWebRequestFileDownload(timeout, handle);
            FileDownloadResult result;
            if (fileSize == 0)
            {
                // 获取文件大小
                result = await downloader.DownloadFile(downloadUrl, downloadTempFilePath);
            }
            else
            {
                result = await downloader.DownloadFile(downloadUrl, downloadTempFilePath, fileSize);
            }

            if (result != UnityWebRequestFileDownload.FileDownloadResult.Success)
            {
                return result;
            }
            // 验证hash
            if (ComputeMD5(downloadTempFilePath) != md5)
            {
                // 删除临时文件夹
                Directory.Delete(downloadTempPath, true);
                downloader.InvokeDownloadHandleFailed();
                return UnityWebRequestFileDownload.FileDownloadResult.VerifyError;
            }
            // 移动到目标路径
            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
            }
            File.Move(downloadTempFilePath, targetFilePath);
            // 删除临时文件夹
            Directory.Delete(downloadTempPath, true);
            return UnityWebRequestFileDownload.FileDownloadResult.Success;
        }

        private static string ComputeMD5(string filePath)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // 使用UNC路径解决windows下路径过长(超过260字符)导致FileStream触发FileNotFoundException异常的问题
            filePath = @"\\?\" + filePath;
#endif
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return StreamMD5(fs);
            }
        }

        /// <summary>
        /// 获取数据流的MD5
        /// </summary>
        private static string StreamMD5(Stream stream)
        {
            using (var provider = MD5.Create())
            {
                byte[] hashBytes = provider.ComputeHash(stream);
                string result = BitConverter.ToString(hashBytes);
                result = result.Replace("-", "");
                return result.ToLower();
            }

        }

    }

}
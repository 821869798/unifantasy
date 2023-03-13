using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniFan.Res.Editor
{
    /// <summary>
    /// 打包资源自动分包
    /// </summary>
    public class BuildResSpliter
    {
        /// <summary>
        /// 整包限制的资源大小
        /// </summary>
        const long LimitResSizeAndroid = 1900 * 1024 * 1024;
        /// <summary>
        /// ios商店限制包体大小不超4g
        /// </summary>
        const long LimitResSizeIOS = (long)4000 * 1024 * 1024;

        static long LimitResSize
        {
            get
            {
#if UNITY_ANDROID
                return LimitResSizeAndroid;
#elif UNITY_IOS
                return LimitResSizeIOS;
#else
                return 0;
#endif
            }
        }

        /// <summary>
        /// 分包时，按顺序移除的路径
        /// </summary>
        static string[] ResSplitRemovePath = {
            PathConsts.MediaPath,
            Path.Combine(Consts.AssetbundleLoadPath,"res","images"),
        };

        /// <summary>
        /// 分包资源的绝对路径
        /// </summary>
        static readonly string ResSplitPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ResSplit"));

        /// <summary>
        /// 资源分离的数据
        /// </summary>
        class ResSplitInfo
        {
            /// <summary>
            /// 要排除的文件夹
            /// </summary>
            public List<string> excludeDirPathList = new List<string>();
            /// <summary>
            /// 要排除的文件
            /// </summary>
            public List<FileInfo> excludeFileList = new List<FileInfo>();
        }

        /// <summary>
        /// 生成需要分离的文件信息
        /// </summary>
        static ResSplitInfo GenResSplit(string resPathRoot)
        {
            Debug.Log("Start GenResSplit");

            //资源文件中,不包含移除目录中的文件
            var fileInfoList = new List<FileInfo>();

            var splitInfo = new ResSplitInfo();

            //统计目录大小
            var resSplitRemovePathSize = new Dictionary<string, long>();

            //初始化待移除目录
            var removePathArray = new string[ResSplitRemovePath.Length];
            for (int i = 0; i < ResSplitRemovePath.Length; i++)
            {
                var fullPath = Path.GetFullPath(Path.Combine(resPathRoot, ResSplitRemovePath[i]));
                removePathArray[i] = fullPath;
                resSplitRemovePathSize[fullPath] = 0;
            }

            //总大小
            var totalSize = FileTools.GetDirectorySize(resPathRoot, true, fileInfo =>
            {
                //在带移除的路径中
                var inRemovePath = false;
                foreach (var pathPre in removePathArray)
                {
                    if (fileInfo.FullName.StartsWith(pathPre))
                    {
                        //计算待移除文件夹的大小
                        resSplitRemovePathSize[pathPre] += fileInfo.Length;
                        inRemovePath = true;
                        break;
                    }
                }

                //不在移除目录中的文件
                if (!inRemovePath)
                    fileInfoList.Add(fileInfo);
            });

            Debug.Log($"totalSize:{totalSize}");

            #region 进行多余文件的剔除

            //1.按顺序逐个移除资源目录
            var removePathArrayIdx = 0;
            while (totalSize > LimitResSize && removePathArrayIdx < removePathArray.Length)
            {
                var path = removePathArray[removePathArrayIdx];
                var dirSize = resSplitRemovePathSize[path];
                if (dirSize > 0)
                {
                    totalSize -= resSplitRemovePathSize[path];
                    splitInfo.excludeDirPathList.Add(path);
                }

                removePathArrayIdx++;
            }

            //2.剩余资源按照固定顺序移除
            if (totalSize > LimitResSize)
                fileInfoList.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
            var fileInfoListIdx = 0;
            while (totalSize > LimitResSize && fileInfoListIdx < fileInfoList.Count)
            {
                var file = fileInfoList[fileInfoListIdx];
                totalSize -= file.Length;
                splitInfo.excludeFileList.Add(file);
                fileInfoListIdx++;
            }

            Debug.Log($"End size:{totalSize}");
            if (totalSize > LimitResSize)
            {
                Debug.LogError($"totalSize({totalSize}) > LimitResSize({LimitResSize})");
            }

            #endregion

            return splitInfo;
        }

        /// <summary>
        /// 多余资源分离,在打包之前
        /// </summary>
        /// <param name="isDelete">不移动资源，而是删除资源</param>
        [MenuItem("GameEditor/ResSplit/ResSplitOnPreprocessBuild")]
        public static void ResSplitOnPreprocessBuild()
        {
            if (Directory.Exists(ResSplitPath))
                Directory.Delete(ResSplitPath, true);

            var splitInfo = GenResSplit(Application.streamingAssetsPath);

            var streamingAssetsPathAbsolute = Path.GetFullPath(Application.streamingAssetsPath);

            //移除多余的资源
            foreach (var dirPath in splitInfo.excludeDirPathList)
            {
                var tarPath = Path.GetFullPath(dirPath).Replace(streamingAssetsPathAbsolute, ResSplitPath);
                FileTools.MoveDir(dirPath, tarPath);
            }

            foreach (var fileInfo in splitInfo.excludeFileList)
            {
                var tarPath = fileInfo.FullName.Replace(streamingAssetsPathAbsolute, ResSplitPath);
                FileTools.MoveFile(fileInfo.FullName, tarPath, true);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 压缩分包的文件夹
        /// </summary>
        [MenuItem("GameEditor/ResSplit/ZipResSplitTest")]
        static void ZipResSplitTest()
        {
            ZipResSplit($"{ResSplitPath}.zip");
        }

        public static void ZipResSplit(string outPath)
        {
            if (Directory.Exists(ResSplitPath))
            {
                var ok = ZipDirectory(ResSplitPath, outPath);
                Debug.Log($"ZipResSplit success : {ok}");
            }
            else
            {
                Debug.Log("ResSplitPath not exists, cancel ZipResSplit");
            }
        }

        /// <summary>
        /// 把分包的资源移动回来
        /// </summary>
        [MenuItem("GameEditor/ResSplit/MoveBackReSplits")]
        public static void MoveBackReSplits()
        {
            FileTools.MoveDir(ResSplitPath, Application.streamingAssetsPath);
            AssetDatabase.Refresh();
            Debug.Log("MoveBackReSplits End.");
        }

        #region 压缩

        /// <summary>   
        /// 递归压缩文件夹的内部方法,忽略meta  
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipStream">压缩输出流</param>   
        /// <param name="parentFolderName">此文件夹的上级文件夹</param>   
        /// <returns></returns>   
        static bool ZipDirectory(string folderToZip, ZipOutputStream zipStream, string parentFolderName)
        {
            bool result = true;
            string[] folders, files;
            ZipEntry ent = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();

            try
            {
                files = Directory.GetFiles(folderToZip);
                foreach (string file in files)
                {
                    //忽略meta
                    if (Path.GetExtension(file).ToLower().Equals(".meta"))
                        continue;

                    fs = File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    ent = new ZipEntry(file.Substring(parentFolderName.Length + 1));
                    ent.DateTime = DateTime.Now;
                    ent.Size = fs.Length;

                    fs.Close();

                    crc.Reset();
                    crc.Update(buffer);

                    ent.Crc = crc.Value;
                    zipStream.PutNextEntry(ent);
                    zipStream.Write(buffer, 0, buffer.Length);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                ent = null;
            }

            folders = Directory.GetDirectories(folderToZip);
            foreach (string folder in folders)
                if (!ZipDirectory(folder, zipStream, parentFolderName))
                    return false;

            return result;
        }

        /// <summary>   
        /// 压缩文件夹,忽略meta    
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipedFile">压缩文件完整路径</param>   
        /// <param name="password">密码</param>   
        /// <returns>是否压缩成功</returns>   
        static bool ZipDirectory(string folderToZip, string zipedFile, string password)
        {
            bool result = false;
            if (!Directory.Exists(folderToZip))
                return result;

            var zipedFileDirPath = Path.GetDirectoryName(zipedFile);
            if (!Directory.Exists(zipedFileDirPath))
                Directory.CreateDirectory(zipedFileDirPath);

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedFile));
            zipStream.SetLevel(6);
            if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

            result = ZipDirectory(folderToZip, zipStream, folderToZip);

            zipStream.Finish();
            zipStream.Close();

            return result;
        }

        /// <summary>   
        /// 压缩文件夹,忽略meta
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipedFile">压缩文件完整路径</param>   
        /// <returns>是否压缩成功</returns>   
        public static bool ZipDirectory(string folderToZip, string zipedFile)
        {
            bool result = ZipDirectory(folderToZip, zipedFile, null);
            return result;
        }

        #endregion


        /// <summary>
        /// 把分离出来的多余资源和md5文件拷贝到指定目录,热更资源自动打包使用
        /// </summary>
        public static void ResSplitCopy()
        {
            var splitArgs = ParseCommandLine();

            var splitInfo = GenResSplit(splitArgs.sourceVersionPath);

            var sourcePathAbsolute = Path.GetFullPath(splitArgs.sourceVersionPath);

            //清空目标目录
            FileTools.DeleteDir(splitArgs.copyTargetPath, false);

            //拷贝多余的资源到目标目录
            foreach (var dirPath in splitInfo.excludeDirPathList)
            {
                var tarPath = Path.GetFullPath(dirPath).Replace(sourcePathAbsolute, splitArgs.copyTargetPath);
                FileTools.DirectoryCopy(dirPath, tarPath, true);
            }

            foreach (var fileInfo in splitInfo.excludeFileList)
            {
                var tarPath = fileInfo.FullName.Replace(sourcePathAbsolute, splitArgs.copyTargetPath);
                FileTools.CopyFile(fileInfo.FullName, tarPath);
            }

            //拷贝md5文件
            var md5File = "resMd5.json";
            FileTools.CopyFile(Path.Combine(splitArgs.sourceVersionPath, md5File), Path.Combine(splitArgs.copyTargetPath, md5File));

            Debug.Log("ResSplitCopy complete");
        }

        #region 解析命令行

        class ResSplitArgs
        {
            /// <summary>
            /// 版本资源库的路径
            /// </summary>
            public string sourceVersionPath;

            /// <summary>
            /// 要拷贝到的目录
            /// </summary>
            public string copyTargetPath;
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        /// <returns></returns>
        static ResSplitArgs ParseCommandLine()
        {
            ResSplitArgs resSplitArgs = new ResSplitArgs();
            string[] args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                Debug.Log(arg);
                if (TryParseOneArg(arg, "sourceVersionPath|", out var sourceVersionPath))
                {
                    resSplitArgs.sourceVersionPath = sourceVersionPath;
                    continue;
                }
                if (TryParseOneArg(arg, "copyTargetPath|", out var copyTargetPath))
                {
                    resSplitArgs.copyTargetPath = copyTargetPath;
                    continue;
                }

            }
            return resSplitArgs;
        }

        static bool TryParseOneArg(string arg, string prefix, out string result)
        {
            result = null;
            if (arg.StartsWith(prefix))
            {
                result = arg.Substring(prefix.Length).Trim();
                return true;
            }
            return false;
        }

        #endregion
    }
}
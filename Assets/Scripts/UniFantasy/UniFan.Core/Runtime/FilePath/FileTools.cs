using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniFan
{
    public static class FileTools
    {
        public static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, Action<FileInfo> afterCopyFileAction = null)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                Debug.LogError("Source directory doesn't exist");
                return false;
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                var newFile = file.CopyTo(tempPath, true);
                afterCopyFileAction?.Invoke(newFile);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (var subDir in dirs)
                {
                    var tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, true, afterCopyFileAction);
                }
            }

            return true;
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <returns></returns>
        public static bool CopyFile(string srcPath, string tarPath)
        {
            try
            {
                var dirPath = Path.GetDirectoryName(tarPath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                else if (File.Exists(tarPath))
                    File.Delete(tarPath); //覆盖

                File.Copy(srcPath, tarPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        public static bool MoveDir(string srcPath, string tarPath)
        {
            try
            {
                var dir = new DirectoryInfo(srcPath);
                if (!dir.Exists)
                {
                    Debug.LogError("Source directory doesn't exist");
                    return false;
                }

                if (!Directory.Exists(tarPath))
                    Directory.CreateDirectory(tarPath);

                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var tarFilePath = Path.Combine(tarPath, file.Name);
                    //覆盖
                    MoveFile(file.FullName, tarFilePath, false);
                }

                var dirs = dir.GetDirectories();
                foreach (var temDir in dirs)
                {
                    var temTargetPath = Path.Combine(tarPath, temDir.Name);
                    MoveDir(temDir.FullName, temTargetPath);
                }

                TryMoveMeta(srcPath, tarPath);
                Directory.Delete(srcPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="tarPath"></param>
        /// <param name="includeMeta"></param>
        /// <returns></returns>
        public static bool MoveFile(string srcPath, string tarPath, bool includeMeta)
        {
            try
            {
                var dirPath = Path.GetDirectoryName(tarPath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                else if (File.Exists(tarPath))
                    File.Delete(tarPath); //覆盖

                File.Move(srcPath, tarPath);

                if (includeMeta)
                    TryMoveMeta(srcPath, tarPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试移动srcPath 的 meta文件
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="tarPath"></param>
        /// <returns></returns>
        public static void TryMoveMeta(string srcPath, string tarPath)
        {
            var metaPath = $"{srcPath}.meta";
            if (File.Exists(metaPath))
            {
                var targetMetaPath = $"{tarPath}.meta";
                MoveFile(metaPath, targetMetaPath, false);
            }
        }

        //删除文件夹和文件夹下所有文件
        //excludePath : 文件夹或者文件
        public static bool DeleteDir(string srcPath, bool includeSrcPath = true, HashSet<string> excludePath = null)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                if (!dir.Exists)
                    return true;

                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (excludePath != null && excludePath.Contains(i.Name))
                        continue;

                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        if (!includeSrcPath && i.FullName == srcPath)
                            continue;

                        var subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        //如果 使用了 streamreader 在删除前 必须先关闭流 ，否则无法删除 sr.close();
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }


        /// <summary>
        /// 获取文件夹大小
        /// </summary>
        /// <param name="dirPath">目标路径</param>
        /// <returns></returns>
        public static long GetDirectorySize(string dirPath, bool excludeMeta, Action<FileInfo> onAddFileSize = null)
        {
            try
            {
                long size = 0;
                var dirInfo = new DirectoryInfo(dirPath);
                var dirs = dirInfo.GetDirectories();
                var files = dirInfo.GetFiles();
                foreach (var dir in dirs)
                {
                    size += GetDirectorySize(dir.FullName, excludeMeta, onAddFileSize);
                }

                foreach (var file in files)
                {
                    if (excludeMeta && file.Extension.Equals(".meta"))
                        continue;

                    size += file.Length;
                    onAddFileSize?.Invoke(file);
                }

                return size;
            }
            catch (Exception e)
            {
                Debug.LogError($"获取文件大小失败, {e}");
                return 0;
            }
        }


        //从路径读取byte[]
        public static byte[] ReadBytesFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            byte[] result = File.ReadAllBytes(filePath);
            return result;
        }

        //往路径写入byte[]
        public static bool WriteBytesToFile(string filePath, byte[] data)
        {
            try
            {
                if (data != null && data.Length > 0)
                {
                    //文件夹目录
                    var diretoryPath = Path.GetDirectoryName(filePath);

                    if (!Directory.Exists(diretoryPath))
                    {
                        Directory.CreateDirectory(diretoryPath);
                    }

                    File.WriteAllBytes(filePath, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WriteBytesToFile] error : {e}");
                return false;
            }
            return true;
        }

    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UniFan.ResEditor
{
    public static class ABBuilder
    {

        public static bool BuildAssetBundles(List<AssetBundleBuild> builds, IResBuildAdapter resBuildAdapter)
        {

            string outputPath = Path.Combine(ABBuildConsts.AssetBundlesOutputPath, ABBuildConsts.GetPlatformName());



            if (builds == null || builds.Count == 0)
            {
                EditorUtility.DisplayDialog("打包错误", "打包的资源数量为0!", "确认");
                return false;
            }

            var ok = resBuildAdapter.BuildAssetBundles(outputPath, builds.ToArray(), EditorUserBuildSettings.activeBuildTarget);
            if (!ok)
            {
                EditorUtility.DisplayDialog("打包错误", "请查看控制台信息", "确认");
                return false;
            }
            //移动manifest文件
            string manifestAsset = Path.Combine(outputPath, ABBuildConsts.GetPlatformName());
            string manifestText = Path.Combine(outputPath, ABBuildConsts.GetPlatformName() + ".manifest");
            string manifestAssetDst = Path.Combine(outputPath, ABBuildConsts.ManifestFilePath);
            string manifestTextDst = Path.Combine(outputPath, ABBuildConsts.ManifestFilePath + ".manifest");
            if (File.Exists(manifestAsset))
            {
                if (File.Exists(manifestAssetDst))
                {
                    File.Delete(manifestAssetDst);
                }
                FileUtil.MoveFileOrDirectory(manifestAsset, manifestAssetDst);
            }
            if (File.Exists(manifestText))
            {
                if (File.Exists(manifestTextDst))
                {
                    File.Delete(manifestTextDst);
                }
                FileUtil.MoveFileOrDirectory(manifestText, manifestTextDst);
            }
            return true;
        }

        static string GetAbOutputPath()
        {
            return Path.Combine(ABBuildConsts.AssetBundlesOutputPath, ABBuildConsts.GetPlatformName());
        }

        public static bool CreateNewAssetBundleDirectory(bool deleteOld)
        {
            string outputPath = GetAbOutputPath();
            try
            {
                if (Directory.Exists(outputPath))
                {
                    if (!deleteOld)
                    {
                        return true;
                    }
                    Directory.Delete(outputPath, true);
                }
                Directory.CreateDirectory(outputPath);
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("打包错误", "打包目标路径文件夹被占用,请关闭后重试!\nerror:" + e.ToString(), "确认");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 拷贝ab资源到ab打包输出目录，用来拷贝增量打包的资源
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns>出错返回false</returns>
        public static bool CopyAssetBundle2OutputPath(string sourceDirPath)
        {
            var outputPath = GetAbOutputPath();
            try
            {
                if (!Directory.Exists(sourceDirPath))
                {
                    Debug.LogError($"CopyAssetBundle2OutputPath Error: sourceDirPath is not exist, {sourceDirPath}");
                    return false;
                }
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                FileTools.DirectoryCopy(sourceDirPath, outputPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"CopyAssetBundle2OutputPath Error: {e}");
                return false;
            }

            return true;
        }
    }
}

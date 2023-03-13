using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UniFan.Res.Editor
{
    public static class ABBuilder
    {

        public static bool BuildAssetBundles(List<AssetBundleBuild> builds, bool isABEncrypt,bool isIncrement, ref AssetBundleManifest manifest)
        {

            string outputPath = Path.Combine(Consts.AssetBundlesOutputPath, Consts.GetPlatformName());

            var options = BuildAssetBundleOptions.None;

            options |= BuildAssetBundleOptions.ChunkBasedCompression;

            if (!isIncrement)
            {
                //非增量打包方式
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            options |= BuildAssetBundleOptions.DeterministicAssetBundle;

            //设置AssetBundle加密
            if (isABEncrypt)
            {
                Type typeBuildAssetBundleOptions = typeof(BuildAssetBundleOptions);
                var fieldEnableProtection = typeBuildAssetBundleOptions.GetField("EnableProtection");
                if (fieldEnableProtection != null)
                {
                    var valueEnableProtection = System.Convert.ToInt32(fieldEnableProtection.GetValue(null));
                    options |= (BuildAssetBundleOptions)valueEnableProtection;
                }
                AssetBundleUtility.SetAssetBundleEncryptKey(AssetBundleUtility.GetAssetBundleKey());
            }
            else
            {
                AssetBundleUtility.SetAssetBundleEncryptKey(null);
            }

            if (builds == null || builds.Count == 0)
            {
                EditorUtility.DisplayDialog("打包错误", "打包的资源数量为0!", "确认");
                return false;
            }
            else
            {
                manifest = BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), options, EditorUserBuildSettings.activeBuildTarget);
            }
            //移动manifest文件
            string manifestAsset = Path.Combine(outputPath, Consts.GetPlatformName());
            string manifestText = Path.Combine(outputPath, Consts.GetPlatformName() + ".manifest");
            string manifestAssetDst = Path.Combine(outputPath, Consts.ManifestFilePath);
            string manifestTextDst = Path.Combine(outputPath, Consts.ManifestFilePath + ".manifest");
            if (File.Exists(manifestAsset))
            {
                if(File.Exists(manifestAssetDst))
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
            return Path.Combine(Consts.AssetBundlesOutputPath, Consts.GetPlatformName());
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

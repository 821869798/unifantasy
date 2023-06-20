using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UniFan.ResEditor
{
    public static class Consts
    {
        //assetbundle资源的扩展名
        public static readonly string BundleExtensionName = ".ab";

        //打包的目标路径
        public const string AssetBundlesOutputPath = "AssetBundles";

        //下列类型文件不处理打包
        public static readonly string[] NoPackedFiles = new string[]
        {
            ".cs",
            ".meta",
            ".dll",
            "",
        };

        public static readonly string[] NoPackedDirs = new string[]
        {
            "NoPack/"
        };

        //下列类型文件的依赖不处理打包
        public static readonly string[] NoPackedDependenciesFiles = new string[]
        {
            ".cs",
            ".meta",
            ".dll",
            "",
        };

        public static readonly string[] NoPackDependFiles = new string[]
        {
            ".fbx",
        };

        //大于等于改数量的时候，是公共资源，需要单独的打包，避免资源冗余
        public const int CommonAssetRelyCount = 2;

        //公共assetbundle前缀
        public const string CommonAssetBundlePrefix = "shared_";

        //储存Unity AssetBundle文件依赖的文件路径
        public const string ManifestFilePath = "manifest";

        //存储自定义格式的所有ab包信息文件
        public const string ResManifestFilePath = "resmainfest.ab";

        //存储自定义格式的ab依赖信息文件
        public const string ResManifestBinaryConfigName = "resmanifest.bytes";

        //打包信息，用于debug或者查看这次打包详情用
        public const string ResBuildReporterName = "build_report.txt";

        //最终存放到streamingassets下的哪个目录
        public const string AssetbundleLoadPath = "bundles";

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        public static string GetPlatformForAssetBundles(BuildTarget platform)
        {
            if (platform == BuildTarget.Android)
            {
                return "Android";
            }
            if (platform == BuildTarget.iOS)
            {
                return "iOS";
            }
            if (platform == BuildTarget.tvOS)
            {
                return "tvOS";
            }
            if (platform == BuildTarget.WebGL)
            {
                return "WebGL";
            }
            if (platform == BuildTarget.StandaloneWindows || platform == BuildTarget.StandaloneWindows64)
            {
                return "Windows";
            }
            if (platform == BuildTarget.StandaloneOSX)
            {
                return "OSX";
            }
            return null;
        }

        public static readonly string[] BuildCullingLangTypeNames = System.Enum.GetNames(typeof(BuildCullingLangType));

        public static BuildCullingLangType LangToCullingType(eLanguageType languageType)
        {
            switch (languageType)
            {
                case eLanguageType.ZH_CN:
                    return BuildCullingLangType.ZH_CN;
                case eLanguageType.ZH_TW:
                    return BuildCullingLangType.ZH_TW;
                case eLanguageType.EN_US:
                    return BuildCullingLangType.EN_US;
                case eLanguageType.JA_JP:
                    return BuildCullingLangType.JA_JP;
                case eLanguageType.KO_KR:
                    return BuildCullingLangType.KO_KR;
                default:
                    throw new System.Exception("can't to change eLanguageType:" + languageType);
            }
        }
    }
}

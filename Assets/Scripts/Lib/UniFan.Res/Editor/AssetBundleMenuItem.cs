using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UniFan;

namespace UniFan.Res.Editor
{
    public static class AssetBundleMenuItem
    {
        const string EncryptExtension = "_Encrypt";

#if UNITY_2019_4_OR_NEWER
        [MenuItem("GameEditor/AssetBundle/BundleBuildSettingWindow", priority = 1)]
        public static void BuilderRulesSetting()
        {
            BundleBuildSettingWindow wnd = EditorWindow.GetWindow<BundleBuildSettingWindow>();
            wnd.titleContent = new GUIContent("BundleBuildSettingWindow");
        }
#else
        [MenuItem("GameEditor/AssetBundle/BundleBuildSettingWindow(Legacy)")]
        public static void BuilderRulesSettingLegacy()
        {
            BundleBuildSettingLegacyWindow.Open();
        }

#endif


        [MenuItem("GameEditor/AssetBundle/Start Build", priority = 10)]
        public static void StartBuildByMenu()
        {
            if (StartBuild(LanguageGlobal.LanguageEditorMode, true) && EditorUtility.DisplayDialog("提示", "AssetBundle打包完成!\nCopy AssetBundles to StreamingAssets?", "确认", "取消"))
                CopyAssetBundlesToStreamingAssets();
            else
                EditorUtility.ClearProgressBar();
        }

        [MenuItem("GameEditor/AssetBundle/Start Build(增量打包)", priority = 11)]
        public static void StartBuildIncrementByMenu()
        {
            if (StartBuild(LanguageGlobal.LanguageEditorMode, true, false, true) && EditorUtility.DisplayDialog("提示", "AssetBundle打包完成!\nCopy AssetBundles to StreamingAssets?", "确认", "取消"))
                CopyAssetBundlesToStreamingAssets();
            else
                EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// 打包AB
        /// </summary>
        /// <param name="language">当前语言环境</param>
        /// <param name="onlyBuildCurLang">是否是打包当前语言的资源</param>
        /// <param name="isABEncrypt">是否加密AB包</param>
        /// <param name="isIncrement">是否是增量打包模式</param>
        /// <returns></returns>
        public static bool StartBuild(eLanguageType language, bool onlyBuildCurLang, bool isABEncrypt = false, bool isIncrement = false)
        {
            if (!ABBuilder.CreateNewAssetBundleDirectory(!isIncrement))
            {
                return false;
            }
            bool isOk = ABBuildCreator.GetBuilds(language, onlyBuildCurLang, out var builds);
            if (!isOk)
            {
                Debug.LogError("打包AssetBundle失败,请查看控制台!");
                return false;
            }
            AssetBundleManifest manifest = null;
            isOk = ABBuilder.BuildAssetBundles(builds, isABEncrypt, isIncrement, ref manifest);
            if (!isOk || manifest == null)
            {
                Debug.LogError("打包AssetBundle失败");
                return false;
            }
            isOk = ManifestBuilder.BuildManifest(manifest);
            if (!isOk)
            {
                Debug.LogError("打包AssetBundle失败:BuildManifest");
                return false;
            }
            return true;
        }

        public static bool CopyAssetBundlesToPath(string destination)
        {
            try
            {
                string outputPath = Path.Combine(Consts.AssetBundlesOutputPath, Consts.GetPlatformName());

                if (!Directory.Exists(outputPath))
                {
                    Debug.Log("No assetBundle output folder, try to build the assetBundles first.");
                    return false;
                }

                if (Directory.Exists(destination))
                    FileUtil.DeleteFileOrDirectory(destination);
                FileTools.DirectoryCopy(outputPath, destination, true);
            }
            catch (Exception e)
            {
               Debug.LogError(e);
               return false;
            }

            return true;
        }

        [MenuItem("GameEditor/AssetBundle/Copy AssetBundles to StreamingAssets", priority = 20)]
        public static void CopyAssetBundlesToStreamingAssets()
        {
            string destination = Path.Combine(Application.streamingAssetsPath, Consts.AssetbundleLoadPath);

            CopyAssetBundlesToPath(destination);
            AssetDatabase.Refresh();
        }

        [MenuItem("GameEditor/AssetBundle/Zip Encrypted", priority = 21)]
        static void ZipEncrypted()
        {
            var sourcePath = Path.Combine(Consts.AssetBundlesOutputPath, Consts.GetPlatformName()) + EncryptExtension;
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError("No encrypted assetBundle output folder, try to encrypt the assetBundles first.");
                return;
            }

            var fz = new FastZip();
            fz.CreateZip(sourcePath + ".zip", sourcePath, true, "");
            fz = null;

            Debug.Log("Zip complete!");
        }

        #region Special Options

        const string kRuntimeMode = "GameEditor/AssetBundle/Editor Bundle Mode";

        [MenuItem(kRuntimeMode,false,100)]
        public static void ToggleRuntimeMode()
        {
            AssetBundleUtility.ActiveBundleMode = !AssetBundleUtility.ActiveBundleMode;
        }

        [MenuItem(kRuntimeMode, true,100)]
        public static bool ToggleRuntimeModeValidate()
        {
            Menu.SetChecked(kRuntimeMode, AssetBundleUtility.ActiveBundleMode);
            return true;
        }

        const string kSimulationAsyncLoad = "GameEditor/AssetBundle/Editor Simulation AsyncLoad";

        [MenuItem(kSimulationAsyncLoad, false,100)]
        public static void ToggleSimulationAsyncLoad()
        {
            AssetBundleUtility.SimulationAsyncLoad = !AssetBundleUtility.SimulationAsyncLoad;
        }

        [MenuItem(kSimulationAsyncLoad, true,100)]
        public static bool ToggleSimulationAsyncLoadValidate()
        {
            Menu.SetChecked(kSimulationAsyncLoad, AssetBundleUtility.SimulationAsyncLoad);
            return true;
        }

        #endregion

    }
}

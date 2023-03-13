using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UniFan;

namespace UniFan.Res.Editor
{
    public static class AssetBundleMenuItem
    {
        const string EncryptExtension = "_Encrypt";

        [MenuItem("GameEditor/AssetBundle/Build Config Setting")]
        public static void BuilderRulesSetting()
        {
            BundleBuildSettingWindow.Open();
        }

        [MenuItem("GameEditor/AssetBundle/Start Build")]
        public static void StartBuildByMenu()
        {
            if (StartBuild(LanguageGlobal.LanguageEditorMode, true) && EditorUtility.DisplayDialog("提示", "AssetBundle打包完成!\nCopy AssetBundles to StreamingAssets?", "确认", "取消"))
                CopyAssetBundlesToStreamingAssets();
            else
                EditorUtility.ClearProgressBar();
        }

        [MenuItem("GameEditor/AssetBundle/Start Build(增量打包)")]
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

        [MenuItem("GameEditor/AssetBundle/Copy AssetBundles to StreamingAssets")]
        public static void CopyAssetBundlesToStreamingAssets()
        {
            string destination = Path.Combine(Application.streamingAssetsPath, Consts.AssetbundleLoadPath);

            CopyAssetBundlesToPath(destination);
            AssetDatabase.Refresh();
        }

        [MenuItem("GameEditor/AssetBundle/Zip Encrypted")]
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

        [MenuItem("GameEditor/SceneShortcut/不保存当前场景并进入Launcher %h")]
        static void OpenSceneLauncher()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Launcher.unity");
        }
        [MenuItem("GameEditor/SceneShortcut/不保存当前场景并进入特效预览场景 %j")]
        static void OpenSceneModTest()
        {
            EditorSceneManager.OpenScene("Assets/Test/ModTest/ModTest02.unity");
        }

        [MenuItem("GameEditor/Clear Editor ProgressBar(防止异常卡住)")]
        public static void ClearEditorProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("GameEditor/清除本地缓存数据/Clear All PlayerPrefs(清除本地保存的数据)")]
        public static void ClearAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("GameEditor/清除本地缓存数据/Clear All EditorPrefs(清除编辑器本地保存的数据)")]
        public static void ClearAllEditorPrefs()
        {
            EditorPrefs.DeleteAll();
        }

        [MenuItem("GameEditor/清除本地缓存数据/Clear All UserData(清除编辑器本地保存的用户数据)")]
        public static void ClearAllUserData()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("状态错误", "运行状态无法调用本方法,请退出后进行调用", "确认");
            }
            else
            {
                try
                {
                    if (Directory.Exists(Consts.EditorPersistentPath))
                    {
                        Directory.Delete(Consts.EditorPersistentPath, true);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        const string kRuntimeMode = "GameEditor/AssetBundle/Editor Bundle Mode";

        [MenuItem(kRuntimeMode)]
        public static void ToggleRuntimeMode()
        {
            AssetBundleUtility.ActiveBundleMode = !AssetBundleUtility.ActiveBundleMode;
        }

        [MenuItem(kRuntimeMode, true)]
        public static bool ToggleRuntimeModeValidate()
        {
            Menu.SetChecked(kRuntimeMode, AssetBundleUtility.ActiveBundleMode);
            return true;
        }
    }
}

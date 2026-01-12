using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoBuild
{
    public class AutoBuildAndroid : AutoBuildPlatformBase
    {
        //国内包的签名文件
        const string KeystoreName = "Tools/Platform/user.keystore";
        const string KeyaliasName = "unifantasy";
        const string AndroidKeystorePass = "123456";
        const string AndroidKeyaliasPass = "123456";

        public override AutoBuildPlatform buildPlatform => AutoBuildPlatform.Android;


        public override void SwitchPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
        }

        public override bool ResetData()
        {
            if (!base.ResetData())
            {
                return false;
            }


            if (buildArgs.buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
                return true;

            // 关闭导出工程
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            //            //设置icon
            //            var textureName = $"Assets/Built-in-Res/Icon/app_icon_{channelType}.png";
            //            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureName);
            //            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { texture });
            //#if UNITY_ANDROID
            //            //Adaptive icon
            //            var icon = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive);
            //            if (ChannelConfig.IsInland(channelType))
            //            {
            //                icon[0].SetTextures(null, null);
            //            }
            //            else
            //            {
            //                var bgTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Built-in-Res/Icon/Adaptive/Background.png");
            //                var fgTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Built-in-Res/Icon/Adaptive/Foreground.png");
            //                icon[0].SetTextures(bgTexture, fgTexture);
            //            }
            //            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive, icon);
            //#endif

            //设置输出路径
            string finalPathDir = Path.Combine(buildArgs.outputPath, buildArgs.buildVersionName);
            if (!Directory.Exists(finalPathDir))
            {
                Directory.CreateDirectory(finalPathDir);
            }

            var filenameExtension = (AutoBuildArgs.AndroidBuildOption)buildArgs.androidBuildOption == AutoBuildArgs.AndroidBuildOption.AABModeArmFull ? ".aab" : ".apk";
            buildArgs.outputFinalPath = Path.Combine(finalPathDir,
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) + "_" + buildArgs.buildVersionName) + filenameExtension;

            //aab bundle
            var androidBuildOption = (AutoBuildArgs.AndroidBuildOption)buildArgs.androidBuildOption;
            EditorUserBuildSettings.buildAppBundle = androidBuildOption == AutoBuildArgs.AndroidBuildOption.AABModeArmFull || androidBuildOption == AutoBuildArgs.AndroidBuildOption.AABModeArmFullAndX86;

            SetAndroidKey(false);


            switch ((AutoBuildArgs.AndroidBuildOption)buildArgs.androidBuildOption)
            {
                case AutoBuildArgs.AndroidBuildOption.Mono:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                    break;
                case AutoBuildArgs.AndroidBuildOption.Il2cppArmFull:
                case AutoBuildArgs.AndroidBuildOption.AABModeArmFull:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                    break;
                case AutoBuildArgs.AndroidBuildOption.Il2cppArmFullAndX86:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64 | AndroidArchitecture.X86;
                    break;
                case AutoBuildArgs.AndroidBuildOption.Il2cpp32:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                    break;
                case AutoBuildArgs.AndroidBuildOption.AABModeArmFullAndX86:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.X86;
                    break;
                case AutoBuildArgs.AndroidBuildOption.Il2cppArm64AndX86:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86;
                    break;
                default:
                    Debug.LogError("no support androidBuildOption :" + buildArgs.androidBuildOption);
                    return false;
            }

            //代码加密(以用腾讯MTP加固替代)
            //var typePlayerSettings = typeof(PlayerSettings);
            //var method = typePlayerSettings.GetMethod("SetEncryptionStateForPlatform", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            //if (buildArgs.enableGameDevelopment || buildArgs.enableUnityDevelopment)
            //{
            //    //开发者模式下不加密
            //    method?.Invoke(null, new object[] { BuildTarget.Android, 0 });
            //}
            //else
            //{
            //    method?.Invoke(null, new object[] { BuildTarget.Android, 1 });
            //}

            return true;
        }

        /// <summary>
        /// 设置Keystore签名密码
        /// </summary>
        public static void SetAndroidKey(bool isOversea)
        {
            //设置Keystore签名密码
            PlayerSettings.Android.keystoreName = KeystoreName;
            PlayerSettings.Android.keystorePass = AndroidKeystorePass;

            PlayerSettings.Android.keyaliasName = KeyaliasName;
            PlayerSettings.Android.keyaliasPass = AndroidKeyaliasPass;
        }

        //public override void DealPlugin(ChannelConfig.ChannelType channelType)
        //{
        //    base.DealPlugin(channelType);

        //    //非dev删除 IngameDebugConsole
        //    var debugPluginAndroidPath = Path.Combine(Application.dataPath, "ThirdParty", "IngameDebugConsole", "Plugins", "Android");
        //    if (buildArgs.enableGameDevelopment)
        //        ProcessSvnCommand($"revert -R \"{debugPluginAndroidPath}\" ");
        //    else
        //        FilePathHelper.DeleteDir(debugPluginAndroidPath);

        //    var pluginAndroidPath = Path.Combine(Application.dataPath, "Plugins", "Android");
        //    var sdkFilePath = Path.Combine(Application.dataPath, "..", "AndroidSDKFiles");

        //    FilePathHelper.DeleteDir(pluginAndroidPath);

        //    switch (channelType)
        //    {
        //        case ChannelConfig.ChannelType.Official:
        //        case ChannelConfig.ChannelType.QATest:
        //        case ChannelConfig.ChannelType.Gray:
        //        case ChannelConfig.ChannelType.Kol:
        //            //官服
        //            ProcessSvnCommand($"revert -R \"{pluginAndroidPath}\" ");
        //            if (channelType == ChannelConfig.ChannelType.QATest || channelType == ChannelConfig.ChannelType.Gray)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_test"), pluginAndroidPath, true);
        //            }
        //            else if (channelType == ChannelConfig.ChannelType.Kol)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_kol"), pluginAndroidPath, true);
        //            }
        //            break;

        //        case ChannelConfig.ChannelType.Bilibili:
        //        case ChannelConfig.ChannelType.BilibiliQATest:
        //        case ChannelConfig.ChannelType.BilibiliKol:
        //        case ChannelConfig.ChannelType.BilibiliGray:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_bilibili"), pluginAndroidPath, true);
        //            if (channelType == ChannelConfig.ChannelType.BilibiliQATest)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_bilibili_test"), pluginAndroidPath, true);
        //            }
        //            else if (channelType == ChannelConfig.ChannelType.BilibiliKol)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_bilibili_kol"), pluginAndroidPath, true);
        //            }
        //            break;

        //        //Android_en 文件夹是海外版plugin的基础，其他海外版文件夹都由此进行叠加覆盖
        //        case ChannelConfig.ChannelType.En:
        //        case ChannelConfig.ChannelType.EnQATest:
        //        case ChannelConfig.ChannelType.EnKol:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_en"), pluginAndroidPath, true);
        //            if (channelType == ChannelConfig.ChannelType.EnQATest)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_en_test"), pluginAndroidPath, true);
        //            }
        //            else if (channelType == ChannelConfig.ChannelType.EnKol)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_en_kol"), pluginAndroidPath, true);
        //            }
        //            break;

        //        case ChannelConfig.ChannelType.Jp:
        //        case ChannelConfig.ChannelType.JpQATest:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_en"), pluginAndroidPath, true);
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_jp"), pluginAndroidPath, true);
        //            if (channelType == ChannelConfig.ChannelType.JpQATest)
        //            {
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Android_jp_test"), pluginAndroidPath, true);
        //            }
        //            break;

        //        case ChannelConfig.ChannelType.KrQATest:
        //        case ChannelConfig.ChannelType.Kr:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Common"), pluginAndroidPath, true);
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Android_kr"), pluginAndroidPath, true);
        //            if (channelType == ChannelConfig.ChannelType.KrQATest)
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Android_kr_test"), pluginAndroidPath, true);
        //            break;

        //        case ChannelConfig.ChannelType.TwQATest:
        //        case ChannelConfig.ChannelType.Tw:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Common"), pluginAndroidPath, true);
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Android_tw"), pluginAndroidPath, true);
        //            if (channelType == ChannelConfig.ChannelType.TwQATest)
        //                FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "Pnsdk", "Android_tw_test"), pluginAndroidPath, true);
        //            break;

        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(channelType), channelType, "[DealPlugin] unsupported channelType");
        //    }
        //}


        protected override bool SetVersion()
        {
            if (!base.SetVersion())
                return false;

            //设置安卓的版本代码
            var nowDate = DateTime.Now;
            if (buildArgs.androidVersionEndNum2 < 0 || buildArgs.androidVersionEndNum2 > 99)
            {
                Debug.LogError($"androidVersionEndNum2({buildArgs.androidVersionEndNum2}) error");
                return false;
            }
            var endNum = buildArgs.androidVersionEndNum2 == 0 ? nowDate.Hour : buildArgs.androidVersionEndNum2;
            PlayerSettings.Android.bundleVersionCode = (nowDate.Year - 2000) * 1000000 + nowDate.Month * 10000 + nowDate.Day * 100 + endNum;

            return true;
        }

        public override void StartBuild()
        {
            if (buildArgs.buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
                return;

            BuildPipeline.BuildPlayer(GetBuildPlayerOptions(buildArgs));
        }
    }
}

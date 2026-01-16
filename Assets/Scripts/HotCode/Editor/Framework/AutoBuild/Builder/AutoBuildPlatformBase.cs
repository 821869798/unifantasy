using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HotCode.Framework;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UniFan.ResEditor;
using UniFan;
using MainEditor;
using HotCode.FrameworkEditor;
using HybridCLR.Editor.Installer;
using MainEditor.HotUpdate;
using UnityEditor.Compilation;

namespace AutoBuild
{
    public abstract class AutoBuildPlatformBase
    {
        //构建的语言环境
        //public eLanguageType buildLang;

        public AutoBuildArgs buildArgs;

        public abstract AutoBuildPlatform buildPlatform { get; }

        /// <summary>
        /// 切换平台 
        /// </summary>
        public abstract void SwitchPlatform();

        /// <summary>
        /// 初始化数据
        /// </summary>
        public virtual bool ResetData()
        {
            // 不要在自动打包代码中使用类似UNITY_ANDROID这样的平台宏，因为切换平台这些需要在下次构建才生效。使用BuildTarget判断

            AutoBuildEntry.UseAutoBuild = true;

            // HyBridCLR如果没有安装，需要安装一下
            InstallerController hybridclrController = new InstallerController();
            if (!hybridclrController.HasInstalledHybridCLR() || hybridclrController.InstalledLibil2cppVersion != hybridclrController.PackageVersion)
            {
                Debug.Log("-------------------------- Start  HybridCLR/Install----------------------------------");
                hybridclrController.InstallDefaultHybridCLR();
                Debug.Log("-------------------------- Finish HybridCLR/Install----------------------------------");
            }
            if (!hybridclrController.HasInstalledHybridCLR())
            {
                // 没安装上
                return false;
            }

            // 初始化宏设置
            InitScriptSymbols();

            // Android il2cpp符号表文件
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;

            // development模式
            EditorUserBuildSettings.development = buildArgs.enableUnityDevelopment;

#if UNITY_EDITOR_WIN
            // 重置变量，防止hybridCLR generate all 失败时杀进程，这个变量没有重置
            UnityEditor.WindowsStandalone.UserBuildSettings.createSolution = false;
#endif

            //buildLang = LanguageGlobal.GetDefaultLanguage(buildArgs.AndroidChannelType);
            //打包时不可设置该值
            //LanguageGlobal.LanguageEditorMode = buildLang;
            //BuildProcessor.buildLanguage = buildLang;
            //BuildProcessor.ipaOutputPath = buildArgs.ipaOutputPath;

            if (buildArgs.enableBuildExcel)
            {

                //todo 根据语言打表
                //if (!TableExportMenu.ExportExcelAll())
                //{
                //    return false;
                //}
            }

            //打包模式
            switch (buildArgs.buildMode)
            {
                case AutoBuildArgs.BuildMode.AllBuild:
                    Debug.Log("-------------------------- Start  HybridCLR/Generate/All----------------------------------");
                    HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                    Debug.Log("-------------------------- Finish  HybridCLR/Generate/All----------------------------------");
                    if (!BuildAssetBundle())
                        return false;
                    //拷贝打包好的AssetBundle到StreamingAssets目录
                    Debug.Log("-------------------------- Start  Copy All AssetBundle To Steaming Assets----------------------------------");
                    AssetBundleMenuItem.CopyAssetBundlesToStreamingAssets();
                    Debug.Log("-------------------------- Finish Copy All AssetBundle To Steaming Assets----------------------------------");
                    /*
                    //拷贝媒体文件
                    Debug.Log("-------------------------- Start  Copy Media Files----------------------------------");
                    if (!MediaBuildProcessor.CopyMedia())
                    {
                        return false;
                    }
                    Debug.Log("-------------------------- Finish Copy Media Files----------------------------------");
                    */

                    #region TODO 拆分资源
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                    {
                        //把多余的资源移除出去,移动平台安装包有大小限制，安卓不超过2g，iOS不超过4g
                        //BuildResSpliter.ResSplitOnPreprocessBuild();
                    }
                    //if (report.summary.platform == BuildTarget.Android || report.summary.platform == BuildTarget.iOS)
                    //{
                    //    //压缩分包资源
                    //    string zipPath;
                    //    if (!string.IsNullOrEmpty(ipaOutputPath) && report.summary.platform == BuildTarget.iOS)
                    //        //放到ipa的输出目录下
                    //        zipPath = Path.Combine(ipaOutputPath, "extraRes.zip");
                    //    else
                    //        zipPath = $"{report.summary.outputPath}_extraRes.zip";

                    //    BuildResSpliter.ZipResSplit(zipPath);
                    //}

                    #endregion

                    // 生成PatchManifest,需要在拆分资源之后,因为里面不需要包括拆分出去的资源
                    Debug.Log("-------------------------------- Start  Generate PatchManifest ---------------------------------------------");
                    if (!PatchEditorHelper.GeneratePatcManifestFileInfo(Application.streamingAssetsPath, buildArgs.GetAppVersion()))
                    {
                        return false;
                    }
                    Debug.Log("-------------------------------- Finish Generate PatchManifest ---------------------------------------------");


                    // 刷新
                    AssetDatabase.Refresh();

                    break;
                case AutoBuildArgs.BuildMode.DirectBuildApp:
                    break;
                case AutoBuildArgs.BuildMode.EmptyApp:

                    Debug.Log("-------------------------- Start  HybridCLR/Generate/All----------------------------------");
                    HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                    Debug.Log("-------------------------- Finish  HybridCLR/Generate/All----------------------------------");

                    //空包删除所有StreamingAssets文件
                    Debug.Log("-------------------------------- Start  Delete All StreamingAssets Files ---------------------------------------------");
                    FilePathHelper.DeleteDir(FilePathHelper.Instance.StreamingAssetsPath, false);
                    Debug.Log("-------------------------------- Finish Delete All StreamingAssets Files ---------------------------------------------");
                    break;

                case AutoBuildArgs.BuildMode.BuildResVersion:

                    #region 热更资源打包相关

                    //打ab
                    if (!BuildAssetBundle())
                        return false;

                    if (buildArgs.copyToResVersion)
                    {
                        Debug.Log("-------------------------------- Start Copy To ResVersion ---------------------------------------------");
                        var destVersionPath = Path.Combine(buildArgs.sourceVersionPath, ABBuildConsts.GetPlatformName(), buildArgs.versionNumber);
                        //删除目录下所有文件
                        if (!FilePathHelper.DeleteDir(destVersionPath))
                        {
                            return false;
                        }

                        // 拷贝ab到版本库文件夹
                        var bundlePath = Path.Combine(destVersionPath, ABBuildConsts.AssetbundleLoadPath);
                        Debug.Log("-------------------------------- Start Copy bundlePath ---------------------------------------------");
                        if (!AssetBundleMenuItem.CopyAssetBundlesToPath(bundlePath))
                        {
                            return false;
                        }
                        // 拷贝音视频到版本库文件夹
                        /*
                        var mediaPath = Path.Combine(destVersionPath, PathConstant.PackedMediaPath);
                        Debug.Log("-------------------------------- Start Copy mediaPath ---------------------------------------------");
                        if (!MediaBuildProcessor.CopyMedia(mediaPath))
                        {
                            return false;
                        }
                        Debug.Log("-------------------------------- Finish Copy To ResVersion ---------------------------------------------");
                        */

                        // 生成PatchManifest
                        Debug.Log("-------------------------------- Start  Generate PatchManifest ---------------------------------------------");
                        if (!PatchEditorHelper.GeneratePatcManifestFileInfo(destVersionPath, buildArgs.versionNumber))
                        {
                            return false;
                        }
                        Debug.Log("-------------------------------- Finish Generate PatchManifest ---------------------------------------------");

                    }

                    #endregion

                    return true;
            }

            //设置版本
            if (!SetVersion())
                return false;

            //设置渠道
            //SetChannel(buildArgs.AndroidChannelType);

            //var channelType = (ChannelConfig.ChannelType)buildArgs.AndroidChannelType;

            //设置应用名字
            //SetProductName(channelType);

            //处理plugin
            //if (buildArgs.dealWithPlugin)
            //    DealPlugin(channelType);

            return true;
        }

        /// <summary>
        /// 打ab
        /// </summary>
        bool BuildAssetBundle()
        {
            //if (buildArgs.enableIncrement && !string.IsNullOrEmpty(buildArgs.incrementAbSourcePath))
            //{
            ////把增量打包的旧ab资源拷贝到当前的ab存放位置，作为增量打包的对比资源
            //Debug.Log("-------------------------------- Start  Copy Old AssetBundle To Bundle OutputPath ---------------------------------------------");
            //if (!ABBuilder.CopyAssetBundle2OutputPath(buildArgs.incrementAbSourcePath))
            //    return false;
            //Debug.Log("-------------------------------- Finish  Copy Old AssetBundle To Bundle OutputPath ---------------------------------------------");
            //}

            //打图集
            Debug.Log("-------------------------------- Start  Make SpriteAtlas ---------------------------------------------");
            SpriteAtlasMakerEditor.MakerAllSpriteAtlas();
            Debug.Log("-------------------------------- Finish Make SpriteAtlas ---------------------------------------------");

            //打Lua的字节码
            //Debug.Log("-------------------------------- Start  Generate Lua ByteCode ---------------------------------------------");
            //GenerateLuaToRes.EnableLocaleTextDebug = buildArgs.enableGameDevelopment;
            //GenerateLuaToRes.GenerateByteCode();
            //Debug.Log("-------------------------------- Finish Generate Lua ByteCode ---------------------------------------------");

            // 打hybrid热更代码
            Debug.Log("-------------------------------- Start  Build HybridCLR HotCode -------------------------------------------");
            if (!BuildHybridCLRCommand.BuildAndCopyABAOTHotUpdateDlls())
            {
                return false;
            }
            Debug.Log("-------------------------------- Finish Build HybridCLR HotCode -------------------------------------------");

            //开始build AssetBundle
            Debug.Log("-------------------------------- Start  Build All AssetBundle ---------------------------------------------");
            IResBuildAdapter adapter = new ResBuildAdapterSBP(LanguageGlobal.LanguageEditorMode, buildArgs.enableIncrement);
            if (!AssetBundleMenuItem.StartBuild(adapter))
            {
                return false;
            }
            Debug.Log("-------------------------------- Finish Build All AssetBundle ---------------------------------------------");

            return true;
        }

        /// <summary>
        /// 设置宏定义
        /// 请不要在自动打包代码中使用这些宏，请直接使用buildArgs
        /// </summary>
        protected virtual void InitScriptSymbols()
        {
            if (buildArgs == null)
                return;
            AutoBuildUtility.SetScriptingDefineSymbolActive("GameDev", buildArgs.enableGameDevelopment);
            //SetScriptingDefineSymbolActive(MTPEditorMenu.MTPSDKSysbolDefine, buildArgs.enableMTP);

            //强制编译
            CompilationPipeline.RequestScriptCompilation();
        }

        /// <summary>
        /// 开始打包
        /// </summary>
        public abstract void StartBuild();


        /// <summary>
        /// 获取BuildPlayerOptions
        /// </summary>
        /// <param name="buildArgs"></param>
        /// <returns></returns>
        protected BuildPlayerOptions GetBuildPlayerOptions(AutoBuildArgs buildArgs)
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.locationPathName = buildArgs.outputFinalPath;
            options.scenes = AutoBuildUtility.GetBuildScenes();
            options.target = EditorUserBuildSettings.activeBuildTarget;
            options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            options.options = BuildOptions.None;
            if (buildArgs.enableUnityDevelopment)
            {
                options.options |= BuildOptions.Development;
            }
            return options;
        }


        //设置版本
        protected virtual bool SetVersion()
        {

            PlayerSettings.bundleVersion = buildArgs.GetAppVersion();

            return true;
        }

        /// <summary>
        /// 处理plugin
        /// </summary>
        //public virtual void DealPlugin(ChannelConfig.ChannelType channelType)
        //{

        //}


        public static void UseOfficialLogo()
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[]
            {
                PlayerSettings.SplashScreenLogo.Create(2f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo.png")),
                PlayerSettings.SplashScreenLogo.Create(2.5f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_final.png")),
            };
        }

        public static void UseBilibiliLogo()
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[]
            {
                PlayerSettings.SplashScreenLogo.Create(2f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_bilibili.png")),
                PlayerSettings.SplashScreenLogo.Create(2f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo.png")),
                PlayerSettings.SplashScreenLogo.Create(2.5f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_final.png")),
            };
        }

        public static void UseOfficialJAJPLogo()
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[]
            {
                PlayerSettings.SplashScreenLogo.Create(2.5f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_JA_JP.png")),
            };
        }

        public static void UseOfficialENUSLogo()
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[]
            {
                PlayerSettings.SplashScreenLogo.Create(2.5f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_EN_US.png")),
            };
        }

        public static void UseHaowanLogo()
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[]
            {
                PlayerSettings.SplashScreenLogo.Create(2.5f,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Built-in-Res/Images/app_logo_haowan.png")),
            };
        }

    }

}

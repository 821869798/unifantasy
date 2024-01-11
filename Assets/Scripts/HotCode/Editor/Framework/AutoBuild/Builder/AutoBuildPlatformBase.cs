using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UniFan.ResEditor;
using UniFan;
using MainEditor;
using HotCode.FrameworkEditor;
using HybridCLR.Editor.Installer;






#if UNITY_2019_3_OR_NEWER
using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER
using System.Reflection;
#endif

namespace AutoBuild
{
    public abstract class AutoBuildPlatformBase
    {
        //构建的语言环境
        //public eLanguageType buildLang;

        public AutoBuildArgs buildArgs;
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
            buildArgs = AutoBuildArgs.ParseFromCommandLine();

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

            //buildLang = LanguageGlobal.GetDefaultLanguage(buildArgs.AndroidChannelType);
            //打包时不可设置该值
            //LanguageGlobal.LanguageEditorMode = buildLang;
            //BuildProcessor.buildLanguage = buildLang;
            //BuildProcessor.ipaOutputPath = buildArgs.ipaOutputPath;

            //包内媒体资源模式
            //MediaBuildProcessor.MediaBuildIn.Value = true;

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
                    break;
                case AutoBuildArgs.BuildMode.NoAssetBundle:
                    break;
                case AutoBuildArgs.BuildMode.EmptyApp:

                    Debug.Log("-------------------------- Start  HybridCLR/Generate/All----------------------------------");
                    HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                    Debug.Log("-------------------------- Finish  HybridCLR/Generate/All----------------------------------");

                    //删除所有StreamingAssets文件
                    Debug.Log("-------------------------------- Start  Delete All StreamingAssets Files ---------------------------------------------");
                    FilePathHelper.DeleteDir(FilePathHelper.Instance.StreamingAssetsPath, false);

                    //包内媒体资源模式
                    //MediaBuildProcessor.MediaBuildIn.Value = false;
                    break;

                case AutoBuildArgs.BuildMode.BuildResVersion:

                    #region 热更资源打包相关

                    ////打ab
                    //if (buildArgs.BuildResVersionAb)
                    //{
                    //    if (!BuildAssetBundle())
                    //        return false;
                    //}

                    ////拷贝资源到版本库
                    //if (buildArgs.CopyToResVersion)
                    //{
                    //    Debug.Log("-------------------------------- Start Copy To ResVersion ---------------------------------------------");

                    //    //删除目录下所有文件
                    //    if (!FilePathHelper.DeleteDir(buildArgs.sourceVersionPath))
                    //        return false;

                    //    var bundlePath = Path.Combine(buildArgs.sourceVersionPath, Consts.AssetbundleLoadPath);
                    //    Debug.Log("-------------------------------- Start Copy bundlePath ---------------------------------------------");
                    //    if (!AssetBundleMenuItem.CopyAssetBundlesToPath(bundlePath))
                    //        return false;

                    //    var mediaPath = Path.Combine(buildArgs.sourceVersionPath, PathConsts.MediaPath);
                    //    Debug.Log("-------------------------------- Start Copy mediaPath ---------------------------------------------");
                    //    if (!BuildProcessor.CopyMedia(mediaPath))
                    //        return false;

                    //    Debug.Log("-------------------------------- Finish Copy To ResVersion ---------------------------------------------");
                    //}

                    #endregion

                    return true;
            }

            //设置版本
            if (!SetResVersion(buildArgs.versionNumber))
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
#if UNITY_2019_3_OR_NEWER
            CompilationPipeline.RequestScriptCompilation();
#elif UNITY_2017_1_OR_NEWER
            var editorAssembly = Assembly.GetAssembly(typeof(Editor));
            var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
            dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
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
        protected virtual bool SetResVersion(string resVersion)
        {
            var version = new Version(resVersion);

            //var versionJson = new LocalVersionJson()
            //{
            //    version = resVersion,
            //    internalResNum = 0
            //};

            //var versionTextPath = FilePathHelper.Instance.GetStreamingPath(HotUpdateController.VersionFileName);
            //versionJson.SaveWrite(versionTextPath);

            PlayerSettings.bundleVersion = string.IsNullOrEmpty(buildArgs.appVersionNumber) ? version.ToString(3) : buildArgs.appVersionNumber;

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

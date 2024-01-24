using HybridCLR.Editor.Installer;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoBuild
{
    public static class AutoBuildEntry
    {
        public static bool UseAutoBuild = false;


        public static bool AutoBuildLogic(AutoBuildPlatformBase builder)
        {
            builder.SwitchPlatform();
            if (builder.ResetData())
            {
                builder.StartBuild();
                return true;
            }
            return false;
        }

        private static void AutoBuildMenu(AutoBuildPlatformBase builder)
        {
            if (AutoBuildLogic(builder))
            {
                EditorUtility.DisplayDialog("Info", "Build Complete", "OK");
                EditorUtility.RevealInFinder(AutoBuildArgs.DefaultBuildOutputPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Build Failed", "OK");
            }
        }


        [MenuItem("GameEditor/AutoBuild/BuildWindows")]
        static void BuildWindowsMenu()
        {
            AutoBuildMenu(new AutoBuildWindows());
        }

        public static void BuildWindows()
        {
            if (!AutoBuildLogic(new AutoBuildWindows()))
            {
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("GameEditor/AutoBuild/BuildAndroid")]
        static void BuildAndroidMenu()
        {
            AutoBuildMenu(new AutoBuildAndroid());
        }

        public static void BuildAndroid()
        {
            if (!AutoBuildLogic(new AutoBuildAndroid()))
            {
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("GameEditor/AutoBuild/BuildiOS")]
        static void BuildiOSMenu()
        {
            AutoBuildMenu(new AutoBuildiOS());
        }
        
        public static void BuildiOS()
        {
            if (!AutoBuildLogic(new AutoBuildiOS()))
            {
                EditorApplication.Exit(1);
            }
        }
        
        [MenuItem("GameEditor/AutoBuild/BuildMacOS")]
        static void BuildMacOSMenu()
        {
            AutoBuildMenu(new AutoBuildMacOS());
        }

        public static void BuildMacOS()
        {
            if (!AutoBuildLogic(new AutoBuildMacOS()))
            {
                EditorApplication.Exit(1);
            }
        }

        /* 
        // 废弃了，不需要此操作了
        [MenuItem("GameEditor/AutoBuild/FirstSetupBuild")]
        static void FirstSetupBuildMenu()
        {
            FirstSetupHybridCLRLogic();
        }

        public static void FirstSetupHybridCLR()
        {
            if (!FirstSetupHybridCLRLogic())
            {
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// 第一次拉下工程，需要setup安装HybridCLR，并且用il2cpp打包一次才行
        /// </summary>
        /// <returns></returns>
        public static bool FirstSetupHybridCLRLogic()
        {
            InstallerController hybridclrController = new InstallerController();
            if (!hybridclrController.HasInstalledHybridCLR())
            {
                hybridclrController.InstallDefaultHybridCLR();
            }
            if (!hybridclrController.HasInstalledHybridCLR())
            {
                // 没安装上
                return false;
            }

            return true;

            //return HybridCLRBuildOnce();
        }

        static bool HybridCLRBuildOnce()
        {

            // HybridCLR 需要先il2cpp打包成功一次，生成mscorlib等程序集才能继续打包
#if UNITY_EDITOR_OSX
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            }
#else
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
#endif
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

            HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();


            var tempBuildPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "HybridCLR_FirstBuild");
            var parentPath = Path.GetDirectoryName(tempBuildPath);
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            BuildPlayerOptions options = new BuildPlayerOptions();
            // 构建到Temp目录中
            options.locationPathName = tempBuildPath;
            options.target = EditorUserBuildSettings.activeBuildTarget;
            options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            options.scenes = AutoBuildUtility.GetBuildScenes();
            options.options = BuildOptions.None;
            var report = BuildPipeline.BuildPlayer(options);
            return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        }

        */

    }

}

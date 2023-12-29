using System.IO;
using UnityEditor;

namespace AutoBuild
{
    public class AutoBuildWindows : AutoBuildPlatformBase
    {
        public override void SwitchPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
        }

        public override bool ResetData()
        {
            if (!base.ResetData())
            {
                return false;
            }
            if (buildArgs.buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
                return true;

            switch ((AutoBuildArgs.AndroidBuildOption)buildArgs.androidBuildOption)
            {
                case AutoBuildArgs.AndroidBuildOption.Mono:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    break;
                default:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                    break;
            }

            //设置输出路径
            string finalPathDir = Path.Combine(buildArgs.outputPath, buildArgs.buildVersionName);
            if (!Directory.Exists(finalPathDir))
            {
                Directory.CreateDirectory(finalPathDir);
            }
            buildArgs.outputFinalPath = Path.Combine(finalPathDir, PlayerSettings.applicationIdentifier + "_" + buildArgs.buildVersionName) + ".exe";
            //初始化平台宏
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

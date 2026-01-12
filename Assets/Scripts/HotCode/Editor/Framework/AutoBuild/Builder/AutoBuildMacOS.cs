using System.IO;
using UnityEditor;

namespace AutoBuild
{
    public class AutoBuildMacOS : AutoBuildWindows
    {
        public override AutoBuildPlatform buildPlatform => AutoBuildPlatform.MacOS;
        public override void SwitchPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            }
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
        }

        protected override void SetOutputPath()
        {
            //设置输出路径
            string finalPathDir = Path.Combine(buildArgs.outputPath, buildArgs.buildVersionName);
            if (!Directory.Exists(finalPathDir))
            {
                Directory.CreateDirectory(finalPathDir);
            }
            buildArgs.outputFinalPath = Path.Combine(finalPathDir, PlayerSettings.productName + "_" + buildArgs.buildVersionName) + ".app";
        }
        
    }
}
using System.IO;
using UnityEditor;

namespace AutoBuild
{
    public class AutoBuildiOS : AutoBuildPlatformBase
    {

        public override AutoBuildPlatform buildPlatform => AutoBuildPlatform.iOS;
        public override void SwitchPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;
        }

        public override bool ResetData()
        {
            if (!base.ResetData())
            {
                return false;
            }
            if (buildArgs.buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
                return true;



            //设置icon，iOS的icon有缓存问题，包名相同的使用同一个icon
            //var textureName = $"Assets/Built-in-Res/Icon/app_icon_{textureChannel}.png";
            //Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureName);
            //PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] { texture });


            //设置输出路径
            string finalPathDir = buildArgs.outputPath;
            string parentDir = Path.GetDirectoryName(finalPathDir);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }
            if (Directory.Exists(finalPathDir))
            {
                Directory.Delete(finalPathDir, true);
            }
            buildArgs.outputFinalPath = finalPathDir;


            return true;
        }


        //public override void DealPlugin(ChannelConfig.ChannelType channelType)
        //{
        //    base.DealPlugin(channelType);

        //    var pluginPath = Path.Combine(Application.dataPath, "Plugins", "iOS");
        //    var sdkFilePath = Path.Combine(Application.dataPath, "..", "iOSSDKFiles");

        //    FilePathHelper.DeleteDir(pluginPath);

        //    switch (channelType)
        //    {
        //        case ChannelConfig.ChannelType.Official:
        //        case ChannelConfig.ChannelType.QATest:
        //        case ChannelConfig.ChannelType.Gray:
        //            ProcessSvnCommand($"revert -R \"{pluginPath}\" ");
        //            break;

        //        case ChannelConfig.ChannelType.En:
        //        case ChannelConfig.ChannelType.EnQATest:
        //        case ChannelConfig.ChannelType.Jp:
        //        case ChannelConfig.ChannelType.JpQATest:
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "MicaSDK_oversea", "Plugins"), pluginPath, true);
        //            break;

        //        case ChannelConfig.ChannelType.Kr:
        //        case ChannelConfig.ChannelType.KrQATest:
        //        case ChannelConfig.ChannelType.Tw:
        //        case ChannelConfig.ChannelType.TwQATest:
        //            //心动sdk
        //            FilePathHelper.DirectoryCopy(Path.Combine(sdkFilePath, "MicaSDK_Pn", "Plugins"), pluginPath, true);
        //            break;

        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(channelType), channelType, "[DealPlugin] unsupported channelType");
        //    }
        //}

        public override void StartBuild()
        {
            if (buildArgs.buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
                return;

            BuildPipeline.BuildPlayer(GetBuildPlayerOptions(buildArgs));
        }
    }
}

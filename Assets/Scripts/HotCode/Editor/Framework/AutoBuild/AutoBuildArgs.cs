using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoBuild
{
    public class AutoBuildArgs
    {
        //打包输出的目录
        public string outputPath;
        //最终的打包路径
        public string outputFinalPath;
        /// <summary>
        /// ios的ipa输出目录
        /// </summary>
        public string ipaOutputPath;
        /// <summary>
        /// 打包的资源版本号,如1.0.1.2
        /// </summary>
        public string versionNumber;
        /// <summary>
        /// app版本号，若为空则使用资源版本号的前三位
        /// </summary>
        public string appVersionNumber;
        //版本名字，一般是作为文件名标记
        public string buildVersionName;
        //是否开启unity的开发模式
        public bool enableUnityDevelopment;
        //Game的开发者模式
        public bool enableGameDevelopment;
        /// <summary>
        /// 是否打表
        /// </summary>
        public bool enableBuildExcel;

        /// <summary>
        /// 开启增量打包
        /// </summary>
        public bool enableIncrement;

        /// <summary>
        /// 出包模式
        /// </summary>
        public BuildMode buildMode;

        /// <summary>
        /// il2cpp的编译选项
        /// </summary>
        public AndroidBuildOption androidBuildOption;

        /// <summary>
        /// Android的app的BuildVersion的最后两位手动位，之前位的是时间
        /// </summary>
        public int androidVersionEndNum2;
		
        /// <summary>
        /// 是否把热更资源拷贝到版本库
        /// </summary>
        public bool copyToResVersion;

        /// <summary>
        /// 版本库所在的目录
        /// </summary>
        public string sourceVersionPath;

        //build方式
        public enum BuildMode
        {
            AllBuild = 0,           //全量打包
            NoAssetBundle = 1,      //不打包AssetBundle，直接Build
            EmptyApp = 2,           //打空包，不带资源
            BuildResVersion = 3,    // 打热更资源
        }

        //特殊Android Build选项
        public enum AndroidBuildOption
        {
            Undefine = -1,
            Mono = 0,
            Il2cpp64 = 1,
            AABMode = 2,
            Il2cpp64AndX86 = 3,
            Il2cpp32 = 4,
            AABAndX86 = 5,
        }

        public static readonly string DefaultBuildOutputPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "build_output").Replace('\\', '/');

        private AutoBuildArgs()
        {
            // 默认值
            this.versionNumber = FormatVersion(new Version(PlayerSettings.bundleVersion), 4);
            this.outputPath = DefaultBuildOutputPath;
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                this.outputPath = Path.Combine(DefaultBuildOutputPath, "xcode_project");
            }
            this.buildVersionName = "temp_manual_build";
            this.buildMode = BuildMode.AllBuild;
            this.androidBuildOption = AndroidBuildOption.Il2cpp64AndX86;
            this.enableUnityDevelopment = EditorUserBuildSettings.development;
            this.enableGameDevelopment = AutoBuildUtility.ContainScriptingDefineSymbol("GameDev");
            this.enableBuildExcel = true;
            this.enableIncrement = true;
            this.copyToResVersion = true;
            this.sourceVersionPath = Path.Combine(DefaultBuildOutputPath, "patch_version").Replace('\\', '/');
        }


        public string GetAppVersion()
        {
            var version = new Version(versionNumber);

            var appVersion = string.IsNullOrEmpty(appVersionNumber) ? version.ToString(3) : appVersionNumber;

            return appVersion;
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        /// <returns></returns>
        public static AutoBuildArgs ParseFromCommandLine()
        {
            AutoBuildArgs buildArgs = new AutoBuildArgs();
            string[] args = Environment.GetCommandLineArgs();
            UnityEngine.Debug.Log("build command line arg:" + string.Join(" ", args));
            // 检查参数列表中是否包含 -executeMethod 参数，判断是否是jenkins调用的
            bool isExecuteMethod = false;
            foreach (var arg in args)
            {
                if (arg == "-executeMethod")
                {
                    isExecuteMethod = true;
                    continue;
                }
                if (TryParseOneArg(arg, "outputPath|", out var outputPath))
                {
                    buildArgs.outputPath = outputPath;
                    continue;
                }
                if (TryParseOneArg(arg, "ipaOutputPath|", out var ipaOutputPath))
                {
                    buildArgs.ipaOutputPath = ipaOutputPath;
                    continue;
                }
                if (TryParseOneArg(arg, "buildVersionName|", out var buildVersionName))
                {
                    buildArgs.buildVersionName = buildVersionName;
                    continue;
                }
                if (TryParseOneArg(arg, "versionNumber|", out var versionNumber))
                {
                    buildArgs.versionNumber = versionNumber;
                    continue;
                }
                if (TryParseOneArg(arg, "appVersionNumber|", out var appVersionNumber))
                {
                    buildArgs.appVersionNumber = appVersionNumber;
                    continue;
                }
                if (TryParseOneArg(arg, "buildMode|", out var buildMode))
                {
                    buildArgs.buildMode = (AutoBuildArgs.BuildMode)int.Parse(buildMode);
                    continue;
                }
                if (TryParseOneArg(arg, "enableUnityDevelopment|", out var enableUnityDevelopment))
                {
                    buildArgs.enableUnityDevelopment = bool.Parse(enableUnityDevelopment);
                    continue;
                }
                if (TryParseOneArg(arg, "enableGameDevelopment|", out var enableGameDevelopment))
                {
                    buildArgs.enableGameDevelopment = bool.Parse(enableGameDevelopment);
                    continue;
                }

                if (TryParseOneArg(arg, "enableIncrement|", out var enableIncrement))
                {
                    buildArgs.enableIncrement = bool.Parse(enableIncrement);
                }

                if (TryParseOneArg(arg, "androidBuildOption|", out var androidBuildOption))
                {
                    buildArgs.androidBuildOption = (AutoBuildArgs.AndroidBuildOption)int.Parse(androidBuildOption);
                }
                if (TryParseOneArg(arg, "enableBuildExcel|", out var enableBuildExcel))
                {
                    buildArgs.enableBuildExcel = bool.Parse(enableBuildExcel);
                }
                if (TryParseOneArg(arg, "androidVersionEndNum2|", out var androidVersionEndNum2))
                {
                    buildArgs.androidVersionEndNum2 = int.Parse(androidVersionEndNum2);
                }
            }
            _ = isExecuteMethod;
            return buildArgs;
        }


        private static bool TryParseOneArg(string arg, string prefix, out string result)
        {
            result = null;
            if (arg.StartsWith(prefix))
            {
                result = arg.Substring(prefix.Length).Trim();
                return true;
            }
            return false;
        }


        public static string FormatVersion(Version version, int precision)
        {
            // Validate input
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (precision < 1 || precision > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be between 1 and 4.");
            }

            // Create a new Version with the specified precision
            Version truncatedVersion = new Version(
                version.Major,
                precision >= 2 ? Math.Max(0, version.Minor) : 0,
                precision >= 3 ? Math.Max(0, version.Build) : 0,
                precision == 4 ? Math.Max(0, version.Revision) : 0
            );

            // Convert the truncated version to a string
            return truncatedVersion.ToString(precision);
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoBuild
{
    /// <summary>
    /// 打包工具可视化窗口
    /// </summary>
    public class AutoBuildEditorWindow : EditorWindow
    {
        // 平台选择
        private AutoBuildPlatform selectedPlatform = AutoBuildPlatform.Windows;

        // 打包参数
        private string outputPath;
        private string versionNumber;
        private string appVersionNumber;
        private string buildVersionName;
        private bool enableUnityDevelopment;
        private bool enableGameDevelopment;
        private bool enableBuildExcel = true;
        private bool enableIncrement = true;
        private AutoBuildArgs.BuildMode buildMode = AutoBuildArgs.BuildMode.AllBuild;
        private AutoBuildArgs.AndroidBuildOption androidBuildOption = AutoBuildArgs.AndroidBuildOption.Il2cppArm64AndX86;
        private int androidVersionEndNum2;
        private bool copyToResVersion = true;
        private string sourceVersionPath;

        // UI 滚动位置
        private Vector2 scrollPosition;

        // 折叠面板状态
        private bool showBasicSettings = true;
        private bool showAdvancedSettings = true;
        private bool showAndroidSettings = true;
        private bool showResVersionSettings = false;

        [MenuItem("GameEditor/AutoBuild/自动打包工具", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoBuildEditorWindow>("AutoBuild 打包工具");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadDefaultValues();
        }

        /// <summary>
        /// 加载默认值
        /// </summary>
        private void LoadDefaultValues()
        {
            outputPath = AutoBuildArgs.DefaultBuildOutputPath;
            versionNumber = AutoBuildArgs.FormatVersion(new Version(PlayerSettings.bundleVersion), 4);
            enableUnityDevelopment = EditorUserBuildSettings.development;
            enableGameDevelopment = AutoBuildUtility.ContainScriptingDefineSymbol("GameDev");
            sourceVersionPath = Path.Combine(AutoBuildArgs.DefaultBuildOutputPath, "patch_version").Replace('\\', '/');

            // 根据当前平台设置默认选择和版本标识名
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    selectedPlatform = AutoBuildPlatform.Windows;
                    buildVersionName = "Windows";
                    break;
                case BuildTarget.Android:
                    selectedPlatform = AutoBuildPlatform.Android;
                    buildVersionName = "Android";
                    break;
                case BuildTarget.iOS:
                    selectedPlatform = AutoBuildPlatform.iOS;
                    buildVersionName = "iOS";
                    break;
                case BuildTarget.StandaloneOSX:
                    selectedPlatform = AutoBuildPlatform.MacOS;
                    buildVersionName = "MacOS";
                    break;
                default:
                    buildVersionName = "Windows";
                    break;
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawPlatformSelection();
            EditorGUILayout.Space(10);

            DrawBasicSettings();
            EditorGUILayout.Space(5);

            DrawAdvancedSettings();
            EditorGUILayout.Space(5);

            if (selectedPlatform == AutoBuildPlatform.Android)
            {
                DrawAndroidSettings();
                EditorGUILayout.Space(5);
            }

            if (buildMode == AutoBuildArgs.BuildMode.BuildResVersion)
            {
                DrawResVersionSettings();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(20);
            DrawBuildButtons();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("AutoBuild 打包工具", titleStyle, GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // 显示当前平台信息
            EditorGUILayout.HelpBox($"当前Unity平台: {EditorUserBuildSettings.activeBuildTarget}", MessageType.Info);
        }

        /// <summary>
        /// 绘制平台选择
        /// </summary>
        private void DrawPlatformSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("目标平台", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 40,
                fixedWidth = 90,
                fontSize = 12
            };

            // Windows 按钮
            GUI.backgroundColor = selectedPlatform == AutoBuildPlatform.Windows ? Color.cyan : Color.white;
            if (GUILayout.Button("Windows", buttonStyle))
            {
                selectedPlatform = AutoBuildPlatform.Windows;
                buildVersionName = "Windows";
            }

            // Android 按钮
            GUI.backgroundColor = selectedPlatform == AutoBuildPlatform.Android ? Color.green : Color.white;
            if (GUILayout.Button("Android", buttonStyle))
            {
                selectedPlatform = AutoBuildPlatform.Android;
                buildVersionName = "Android";
            }

            // iOS 按钮
            GUI.backgroundColor = selectedPlatform == AutoBuildPlatform.iOS ? Color.yellow : Color.white;
            if (GUILayout.Button("iOS", buttonStyle))
            {
                selectedPlatform = AutoBuildPlatform.iOS;
                buildVersionName = "iOS";
            }

            // MacOS 按钮
            GUI.backgroundColor = selectedPlatform == AutoBuildPlatform.MacOS ? Color.magenta : Color.white;
            if (GUILayout.Button("MacOS", buttonStyle))
            {
                selectedPlatform = AutoBuildPlatform.MacOS;
                buildVersionName = "MacOS";
            }

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制基础设置
        /// </summary>
        private void DrawBasicSettings()
        {
            showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "基础设置");
            if (showBasicSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                // 打包模式
                buildMode = (AutoBuildArgs.BuildMode)EditorGUILayout.EnumPopup("打包模式", buildMode);
                DrawBuildModeHelp();

                EditorGUILayout.Space(5);

                // 版本号
                versionNumber = EditorGUILayout.TextField("资源版本号", versionNumber);
                appVersionNumber = EditorGUILayout.TextField("App版本号 (可选)", appVersionNumber);
                buildVersionName = EditorGUILayout.TextField("版本标识名", buildVersionName);

                EditorGUILayout.Space(5);

                // 输出路径
                EditorGUILayout.BeginHorizontal();
                outputPath = EditorGUILayout.TextField("输出路径", outputPath);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string path = EditorUtility.OpenFolderPanel("选择输出目录", outputPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        outputPath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制打包模式帮助信息
        /// </summary>
        private void DrawBuildModeHelp()
        {
            string helpText = buildMode switch
            {
                AutoBuildArgs.BuildMode.AllBuild => "全量打包：打AB包 + 打应用",
                AutoBuildArgs.BuildMode.DirectBuildApp => "不打AB包，直接Build应用",
                AutoBuildArgs.BuildMode.EmptyApp => "打空包，不带资源",
                AutoBuildArgs.BuildMode.BuildResVersion => "仅打热更资源，不打应用",
                _ => ""
            };
            if (!string.IsNullOrEmpty(helpText))
            {
                EditorGUILayout.HelpBox(helpText, MessageType.None);
            }
        }

        /// <summary>
        /// 绘制高级设置
        /// </summary>
        private void DrawAdvancedSettings()
        {
            showAdvancedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedSettings, "高级设置");
            if (showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                // 临时增加标签宽度以确保文字完整显示
                var oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 180;

                enableUnityDevelopment = EditorGUILayout.Toggle("Unity Development 模式", enableUnityDevelopment);
                enableGameDevelopment = EditorGUILayout.Toggle("Game Development 模式", enableGameDevelopment);
                enableBuildExcel = EditorGUILayout.Toggle("打表", enableBuildExcel);
                enableIncrement = EditorGUILayout.Toggle("增量打包", enableIncrement);

                EditorGUIUtility.labelWidth = oldLabelWidth;

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制 Android 专属设置
        /// </summary>
        private void DrawAndroidSettings()
        {
            showAndroidSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAndroidSettings, "Android 设置");
            if (showAndroidSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                androidBuildOption = (AutoBuildArgs.AndroidBuildOption)EditorGUILayout.EnumPopup("编译选项", androidBuildOption);
                DrawAndroidBuildOptionHelp();

                androidVersionEndNum2 = EditorGUILayout.IntSlider("版本末位数字 (0-99)", androidVersionEndNum2, 0, 99);
                if (androidVersionEndNum2 == 0)
                {
                    EditorGUILayout.HelpBox("末位为0时，将使用当前小时数", MessageType.Info);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制 Android 编译选项帮助信息
        /// </summary>
        private void DrawAndroidBuildOptionHelp()
        {
            string helpText = androidBuildOption switch
            {
                AutoBuildArgs.AndroidBuildOption.Mono => "Mono: ARMv7 + X86",
                AutoBuildArgs.AndroidBuildOption.Il2cppArmFull => "IL2CPP: ARMv7 + ARM64",
                AutoBuildArgs.AndroidBuildOption.AABModeArmFull => "AAB: ARMv7 + ARM64 (Google Play)",
                AutoBuildArgs.AndroidBuildOption.Il2cppArmFullAndX86 => "IL2CPP: ARMv7 + ARM64 + X86",
                AutoBuildArgs.AndroidBuildOption.Il2cpp32 => "IL2CPP: ARMv7 + X86 (32位)",
                AutoBuildArgs.AndroidBuildOption.AABModeArmFullAndX86 => "AAB: X86",
                AutoBuildArgs.AndroidBuildOption.Il2cppArm64AndX86 => "IL2CPP: ARM64 + X86 (32位)",
                _ => ""
            };
            if (!string.IsNullOrEmpty(helpText))
            {
                EditorGUILayout.HelpBox(helpText, MessageType.None);
            }
        }

        /// <summary>
        /// 绘制热更资源版本设置
        /// </summary>
        private void DrawResVersionSettings()
        {
            showResVersionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showResVersionSettings, "热更资源设置");
            if (showResVersionSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                copyToResVersion = EditorGUILayout.Toggle("拷贝到版本库", copyToResVersion);

                if (copyToResVersion)
                {
                    EditorGUILayout.BeginHorizontal();
                    sourceVersionPath = EditorGUILayout.TextField("版本库路径", sourceVersionPath);
                    if (GUILayout.Button("...", GUILayout.Width(30)))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择版本库目录", sourceVersionPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            sourceVersionPath = path;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制打包按钮
        /// </summary>
        private void DrawBuildButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 参数预览
            EditorGUILayout.LabelField("打包预览", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"平台: {selectedPlatform}");
            EditorGUILayout.LabelField($"模式: {buildMode}");
            EditorGUILayout.LabelField($"版本: {versionNumber}");

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // 重置按钮
            if (GUILayout.Button("重置参数", GUILayout.Width(100), GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要重置所有参数吗？", "确定", "取消"))
                {
                    LoadDefaultValues();
                }
            }

            GUILayout.Space(20);

            // 打包按钮
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("开始打包", GUILayout.Width(150), GUILayout.Height(35)))
            {
                StartBuild();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 打开输出目录按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("打开输出目录", GUILayout.Width(150), GUILayout.Height(25)))
            {
                if (Directory.Exists(outputPath))
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "输出目录不存在", "确定");
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 开始打包
        /// </summary>
        private void StartBuild()
        {
            // 参数验证
            if (string.IsNullOrEmpty(versionNumber))
            {
                EditorUtility.DisplayDialog("错误", "请填写资源版本号", "确定");
                return;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                EditorUtility.DisplayDialog("错误", "请填写输出路径", "确定");
                return;
            }

            // 确认打包
            string message = $"确定要开始打包吗？\n\n" +
                             $"平台: {selectedPlatform}\n" +
                             $"模式: {buildMode}\n" +
                             $"版本: {versionNumber}\n" +
                             $"输出: {outputPath}";

            if (!EditorUtility.DisplayDialog("确认打包", message, "开始打包", "取消"))
            {
                return;
            }

            // 获取对应平台的 Builder
            AutoBuildPlatformBase builder = GetPlatformBuilder();
            if (builder == null)
            {
                EditorUtility.DisplayDialog("错误", "不支持的平台", "确定");
                return;
            }

            // 执行打包
            Debug.Log($"[AutoBuildWindow] 开始打包 - 平台:{selectedPlatform} 模式:{buildMode}");

            bool success = ExecuteBuild(builder);

            if (success)
            {
                EditorUtility.DisplayDialog("成功", "打包完成！", "确定");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "打包失败，请查看Console日志", "确定");
            }
        }

        /// <summary>
        /// 执行打包流程（自定义流程以支持参数传递）
        /// </summary>
        private bool ExecuteBuild(AutoBuildPlatformBase builder)
        {
            // 1. 切换平台
            builder.SwitchPlatform();

            // 2. 手动设置参数（在 ResetData 之前设置，避免被覆盖）
            builder.buildArgs = CreateBuildArgs();

            // 3. 调用 ResetData 但跳过参数解析
            // 由于 ResetData 内部会调用 ParseFromCommandLine 覆盖 buildArgs，
            // 我们需要在 ResetData 之后重新设置
            AutoBuildEntry.UseAutoBuild = true;

            // 调用 ResetData（内部会重新解析参数）
            if (!builder.ResetData())
            {
                return false;
            }

            // 4. 开始构建
            builder.StartBuild();

            return true;
        }

        /// <summary>
        /// 获取平台对应的 Builder
        /// </summary>
        private AutoBuildPlatformBase GetPlatformBuilder()
        {
            return selectedPlatform switch
            {
                AutoBuildPlatform.Windows => new AutoBuildWindows(),
                AutoBuildPlatform.Android => new AutoBuildAndroid(),
                AutoBuildPlatform.iOS => new AutoBuildiOS(),
                AutoBuildPlatform.MacOS => new AutoBuildMacOS(),
                _ => null
            };
        }

        /// <summary>
        /// 创建打包参数
        /// </summary>
        private AutoBuildArgs CreateBuildArgs()
        {
            // 使用反射创建实例（因为构造函数是私有的）
            var args = AutoBuildArgs.GetDefaultArgs(selectedPlatform);

            args.outputPath = outputPath;
            args.versionNumber = versionNumber;
            args.appVersionNumber = appVersionNumber;
            args.buildVersionName = buildVersionName;
            args.enableUnityDevelopment = enableUnityDevelopment;
            args.enableGameDevelopment = enableGameDevelopment;
            args.enableBuildExcel = enableBuildExcel;
            args.enableIncrement = enableIncrement;
            args.buildMode = buildMode;
            args.androidBuildOption = androidBuildOption;
            args.androidVersionEndNum2 = androidVersionEndNum2;
            args.copyToResVersion = copyToResVersion;
            args.sourceVersionPath = sourceVersionPath;

            return args;
        }
    }
}

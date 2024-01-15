using HybridCLR.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AutoBuild
{
    public class GameBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        #region pre build & post build

        public int callbackOrder => 0;

        public static bool IsHybridCLRBuildGenerateAll(string outputPath)
        {
            var baseName = Path.GetFileName(SettingsUtil.HybridCLRDataDir);

            string path = outputPath;
            while (!string.IsNullOrEmpty(path))
            {
                if (Path.GetFileName(path) == baseName)
                {
                    return true;
                }
                path = Path.GetDirectoryName(path);
            }
            return false;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (IsHybridCLRBuildGenerateAll(report.summary.outputPath))
            {
                // 如果是HybridCLR的构建脚本生成调用的BuildPipeline.BuildPlayer，就不处理
                return;
            }

            Debug.Log($"[{nameof(GameBuildProcessor)}] -> {nameof(OnPreprocessBuild)}");

        }


        public static string ipaOutputPath;
        public void OnPostprocessBuild(BuildReport report)
        {
            if (IsHybridCLRBuildGenerateAll(report.summary.outputPath))
            {
                // 如果是HybridCLR的构建脚本生成调用的BuildPipeline.BuildPlayer，就不处理
                return;
            }

            Debug.Log($"[{nameof(GameBuildProcessor)}] -> {nameof(OnPostprocessBuild)}");


            if (report.summary.platform == BuildTarget.iOS)
            {
                XCodePostProcess.PostProcessBuild(report);
            }
        }

        #endregion

        #region Gradle不压缩的文件处理。 已废弃，不需要特殊处理

        static string GradleContent;
        static readonly string GradleFilePath = Path.Combine(Application.dataPath, "Plugins", "Android", "mainTemplate.gradle");
        //不压缩的文件
        const string GradleReplaceContent = ", '.meta', '.txt', 'manifest', '.manifest', '.ab'";

        static void DealAndroidGradle()
        {
            if (!File.Exists(GradleFilePath))
                return;
            try
            {
                //替换文件后缀
                var content = File.ReadAllText(GradleFilePath, Encoding.UTF8);
                GradleContent = content;
                content = content.Replace("**STREAMING_ASSETS**", GradleReplaceContent);
                File.WriteAllText(GradleFilePath, content, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        static void ResumeAndroidGradle()
        {
            File.WriteAllText(GradleFilePath, GradleContent, Encoding.UTF8);
        }

        //先把需要放在StreamingAssets下的文件都放好，然后使用该工具查找生成需要替换的文本内容，并替换GradleReplaceContent
        [MenuItem("GameEditor/BuildTools/Test/查找StreamingAssets目录下用于gradle不压缩的文件", priority = int.MaxValue)]
        static void GenStreamingAssetsSuffix()
        {
            var fileSuffixSet = new HashSet<string>();
            GenStreamingAssetsSuffix(Application.streamingAssetsPath, fileSuffixSet);
            var replaceContent = string.Empty;
            foreach (var suffix in fileSuffixSet)
                replaceContent += $", '{suffix}'";
            Debug.Log($"gradle replaceContent : {replaceContent}");
        }

        //查找目录下所有文件的后缀
        static void GenStreamingAssetsSuffix(string tarPath, ISet<string> fileSuffixSet)
        {
            try
            {
                var dir = new DirectoryInfo(tarPath);
                if (!dir.Exists)
                {
                    Debug.LogError("Source directory doesn't exist");
                    return;
                }

                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var extension = file.Extension;
                    if (string.IsNullOrEmpty(extension))
                        fileSuffixSet.Add(file.Name);
                    else
                        fileSuffixSet.Add(extension);
                }

                var dirs = dir.GetDirectories();
                foreach (var subDir in dirs)
                {
                    GenStreamingAssetsSuffix(subDir.FullName, fileSuffixSet);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion
    }
}

using HybridCLR.Editor;
using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Commands;
using Main;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MainEditor
{
    public static class BuildHybridCLRCommand
    {

        const string kHybridCLREditActive = "HybridCLR/HybridCLR Editor Active";

        [MenuItem(kHybridCLREditActive, false, 2000)]
        public static void ToggleSimulationAsyncLoad()
        {
            HybridCLRUtil.HybridCLREditActive = !HybridCLRUtil.HybridCLREditActive;
        }

        [MenuItem(kHybridCLREditActive, true, 2000)]
        public static bool ToggleSimulationAsyncLoadValidate()
        {
            Menu.SetChecked(kHybridCLREditActive, HybridCLRUtil.HybridCLREditActive);
            return true;
        }

        public static readonly string CodeOutputDir = Path.Combine(Application.dataPath, HybridCLRUtil.CodeDllPath);

        [MenuItem("HybridCLR/BuildCodesAndCopy", false, 3000)]
        public static void BuildAndCopyABAOTHotUpdateDllsMenu()
        {
            BuildAndCopyABAOTHotUpdateDlls();
        }

        public static bool BuildAndCopyABAOTHotUpdateDlls()
        {
            try
            {
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                CompileDllCommand.CompileDll(target);
                CopyABAOTHotUpdateDlls(target);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("HybridCLR/BuildCodesAndCopy failed:" + e);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return false;
        }

        public static void CopyABAOTHotUpdateDlls(BuildTarget target)
        {
            // 删除原来的文件
            DeleteFilesByExtension(CodeOutputDir, "*.bytes");

            CopyAOTAssembliesToCodeAssets();
            CopyHotUpdateAssembliesToCodeAssets();
        }

        public static void CopyAOTAssembliesToCodeAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string fileList = Path.Combine(CodeOutputDir, HybridCLRUtil.AOTMetadataPath, HybridCLRUtil.AotFileListName);
            using (var fs = File.OpenWrite(fileList))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(SettingsUtil.AOTAssemblyNames.Count);

                foreach (var dll in SettingsUtil.AOTAssemblyNames)
                {
                    string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                    if (!File.Exists(srcDllPath))
                    {
                        Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                        continue;
                    }
                    bw.Write($"{dll}.dll.bytes");
                    string dllBytesPath = Path.Combine(CodeOutputDir, HybridCLRUtil.AOTMetadataPath, dll + ".dll.bytes");
                    // strip dll
                    AOTAssemblyMetadataStripper.Strip(srcDllPath, dllBytesPath);
                    //File.Copy(srcDllPath, dllBytesPath, true);
                    Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
                }
            }
        }

        public static void CopyHotUpdateAssembliesToCodeAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);

            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = Path.Combine(CodeOutputDir, dll + ".bytes");
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
        }

        public static void DeleteFilesByExtension(string directoryPath, string searchPattern)
        {
            string[] files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}

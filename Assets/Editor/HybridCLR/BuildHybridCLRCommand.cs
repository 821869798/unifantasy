using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Commands;
using System.IO;

namespace HybridCLR.Editor
{
    public static class BuildHybridCLRCommand
    {

        public static readonly string CodeOutputDir = Path.Combine(Application.dataPath,"Res","Codes") ;


        [MenuItem("HybridCLR/BuildCodesAndCopy")]
        public static void BuildAndCopyABAOTHotUpdateDlls()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            CopyABAOTHotUpdateDlls(target);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static void CopyABAOTHotUpdateDlls(BuildTarget target)
        {
            CopyAOTAssembliesToCodeAssets();
            CopyHotUpdateAssembliesToCodeAssets();
        }

        public static void CopyAOTAssembliesToCodeAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);

            foreach (var dll in SettingsUtil.AOTAssemblyNames)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError($"ab�����AOT����Ԫ����dll:{srcDllPath} ʱ��������,�ļ������ڡ��ü����AOT dll��BuildPlayerʱ�������ɣ������Ҫ���ȹ���һ����ϷApp���ٴ����");
                    continue;
                }
                string dllBytesPath = $"{CodeOutputDir}/{dll}.dll.bytes";
                File.Copy(srcDllPath, dllBytesPath, true);
                Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
            }
        }

        public static void CopyHotUpdateAssembliesToCodeAssets()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);

#if NEW_HYBRIDCLR_API
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
#else
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFiles)
#endif
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{CodeOutputDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
        }
    }

}
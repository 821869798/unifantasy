using System;
using System.Collections.Generic;
using System.IO;
using Main.HotUpdate;
using UnityEditor;
using UnityEngine;

namespace MainEditor.HotUpdate
{
    /// <summary>
    /// 用于打包后生成Patch文件
    /// </summary>
    public static class PatchEditorHelper
    {
        const string kActiveEditorPatchLogic = "GameEditor/PatchLogic/Editor Simulation HotUpdate Patch";

        [MenuItem(kActiveEditorPatchLogic, false, 100)]
        public static void ToggleSimulationAsyncLoad()
        {
            PatchLogicUtility.ActiveEditorPatchLogic.Value = !PatchLogicUtility.ActiveEditorPatchLogic.Value;
        }

        [MenuItem(kActiveEditorPatchLogic, true, 100)]
        public static bool ToggleSimulationAsyncLoadValidate()
        {
            Menu.SetChecked(kActiveEditorPatchLogic, PatchLogicUtility.ActiveEditorPatchLogic);
            return true;
        }
        

        [MenuItem("GameEditor/PatchLogic/Generate PatchManfist In StreamingAssets ", priority = 2)]
        /// <summary>
        /// 测试用
        /// </summary>
        static void GeneratePatchFileListInStreamingAssetsMenu()
        {
            if (GeneratePatcManifestFileInfo(Application.streamingAssetsPath, PlayerSettings.bundleVersion))
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("提示", "生成成功", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "生成失败", "确定");
            }
        }

        /// <summary>
        /// 生成Patch文件列表信息
        /// patchVersion : 如果是包内的就是app版本作为前缀，如果是热更下来的，就是热更版本作为前缀
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="patchVersion"></param>
        /// <returns></returns>
        public static bool GeneratePatcManifestFileInfo(string srcPath, string patchVersion)
        {
            var patchManifestGenerateConfig = new PatchManifestGenerateConfig();
            patchManifestGenerateConfig.filePath = srcPath.Replace('\\', '/');
            patchManifestGenerateConfig.addPrefixPath = "";
            patchManifestGenerateConfig.blackFileExt.Add(".meta");
            patchManifestGenerateConfig.blackFileExt.Add(".manifest");
            patchManifestGenerateConfig.blackFileExt.Add(".DS_Store");
            patchManifestGenerateConfig.blackFileExt.Add(".gitkeep");
            patchManifestGenerateConfig.blackFiles.Add(PatchLogicUtility.VersionFileName);

            try
            {
                string writeFilePath = Path.Combine(srcPath, PatchLogicUtility.PatchManfistRootPath);
                if (Directory.Exists(writeFilePath))
                {
                    Directory.Delete(writeFilePath, true);
                }
                Directory.CreateDirectory(writeFilePath);

                if (!TryGeneratePatchFileList(new List<PatchManifestGenerateConfig>() { patchManifestGenerateConfig }, out var patchManifestData))
                {
                    return false;
                }

                patchManifestData.patchVersion = patchVersion;

                // 写入测试文件
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(patchManifestData);
                var jsonPath = Path.Combine(writeFilePath, PatchLogicUtility.GetPatchManfistJsonFileName(patchVersion));
                File.WriteAllText(jsonPath, json);
                // 写入二进制文件
                var binaryPath = Path.Combine(writeFilePath, PatchLogicUtility.GetPatchManfistFileName(patchVersion));
                using (var fs = new FileStream(binaryPath, FileMode.Create))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        patchManifestData.Write2Binary(bw);
                    }
                }
                // 写入一个二进制文件的hash
                if (!HashUtility.TryFileMD5(binaryPath, out var hashValue))
                {
                    Debug.LogError($"[{nameof(PatchEditorHelper)}|{nameof(GeneratePatcManifestFileInfo)}] TryFileMD5 failed");
                    return false;
                }
                var hashPath = Path.Combine(writeFilePath, PatchLogicUtility.GetPatchManfistHashFileName(patchVersion));
                File.WriteAllText(hashPath, hashValue);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(PatchEditorHelper)}|{nameof(GeneratePatcManifestFileInfo)}] Exception: {e}");
                return false;
            }
        }

        /// <summary>
        /// 通过路径生成文件列表信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="blackFileExts"></param>
        public static bool TryGeneratePatchFileList(List<PatchManifestGenerateConfig> generateConfigs, out PatchManifestData patchManifestData)
        {
            patchManifestData = null;
            if (generateConfigs == null || generateConfigs.Count == 0)
            {
                Debug.Log($"[{nameof(PatchEditorHelper)}|{nameof(TryGeneratePatchFileList)}] generateConfigs is null or empty");
                return false;
            }

            patchManifestData = new PatchManifestData();
            try
            {
                foreach (var g in generateConfigs)
                {
                    GenerateInternal(g, patchManifestData);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(PatchEditorHelper)}|{nameof(TryGeneratePatchFileList)}] Exception: {e}");
            }

            patchManifestData = null;
            return false;
        }

        private static bool GenerateInternal(PatchManifestGenerateConfig manifestGenerate, PatchManifestData patchManifestData)
        {
            if (manifestGenerate == null)
            {
                Debug.Log($"[{nameof(PatchEditorHelper)}|{nameof(GenerateInternal)}] patchManifestGenerate is null");
                return false;
            }

            if (patchManifestData == null)
            {
                Debug.Log($"[{nameof(PatchEditorHelper)}|{nameof(GenerateInternal)}] patchManifestData is null");
                return false;
            }

            if (string.IsNullOrEmpty(manifestGenerate.filePath) || !Directory.Exists(manifestGenerate.filePath))
            {
                Debug.Log($"[{nameof(PatchEditorHelper)}|{nameof(GenerateInternal)}] patchManifestGenerate.filePath is null or not exist");
                return false;
            }

            var filePath = manifestGenerate.filePath.Replace('\\', '/');
            var startLength = filePath.Length + 1;
            var prefixPath = manifestGenerate.addPrefixPath.Replace('\\', '/').TrimStart('/').TrimEnd('/');
            if (!string.IsNullOrEmpty(prefixPath))
            {
                prefixPath += '/';
            }


            var files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileFormat = file.Substring(startLength).Replace('\\', '/');

                var fileExt = Path.GetExtension(fileFormat);
                if (manifestGenerate.blackFileExt.Contains(fileExt))
                {
                    continue;
                }

                if (manifestGenerate.blackFiles.Contains(fileFormat))
                {
                    continue;
                }

                var fileInfo = new PatchFileInfo();
                fileInfo.filePath = prefixPath + fileFormat;
                fileInfo.fileSize = new FileInfo(file).Length;
                if (HashUtility.TryFileMD5(file, out var result))
                {
                    fileInfo.fileMd5 = result;
                }
                else
                {
                    throw new Exception(result);
                }

                patchManifestData.fileInfoList.Add(fileInfo);
            }


            return false;
        }

    }
}

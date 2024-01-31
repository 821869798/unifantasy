using System;
using System.IO;
using UniFan;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Main.HotUpdate
{
    public static class PatchLogicUtility
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            Debug.Log($"Init->{nameof(ActiveEditorPatchLogic)}: {ActiveEditorPatchLogic.Value}");
        }

        // 编辑器模拟热更下载
        public static EditorPreferenceBool ActiveEditorPatchLogic { get; } = new EditorPreferenceBool(nameof(ActiveEditorPatchLogic));
#endif

        public const string VersionFileName = "version.txt";

        public const string PatchManfistRootPath = "patch";

        public const string PatchManfistFileName = "patch_manifest.bytes";

        public const string PatchManfistHashFileName = "patch_manifest.bytes.hash";

        public const string PatchManfistJsonFileName = "patch_manifest_text.json";

        public const string DownloadCachePath = PatchManfistRootPath + "/cache";

        public static string GetPatchManfistFileName(string version)
        {
            return version + "_" + PatchManfistFileName;
        }

        public static string GetPatchManfistHashFileName(string version)
        {
            return version + "_" + PatchManfistHashFileName;
        }

        public static string GetPatchManfistJsonFileName(string version)
        {
            return version + "_" + PatchManfistJsonFileName;
        }

        /// <summary>
        /// 尝试加载本地版本文件
        /// </summary>
        /// <param name="version"></param>
        /// <param name="patchManifestData"></param>
        /// <returns></returns>
        public static bool TryLoadLocalPatchManifestFile(string version, out PatchManifestData patchManifestData)
        {
            patchManifestData = null;
            try
            {

                var patchManifestFilePath = FilePathHelper.Instance.GetPersistentDataPath(PatchLogicUtility.PatchManfistRootPath + "/" + PatchLogicUtility.GetPatchManfistFileName(version));
                if (!File.Exists(patchManifestFilePath))
                {
                    return false;
                }

                using (FileStream ms = new FileStream(patchManifestFilePath, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        patchManifestData = new PatchManifestData();
                        patchManifestData.Read4Binary(br);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                patchManifestData = null;
                Debug.LogError($"[{nameof(PatchLogicUtility)}|{nameof(TryLoadLocalPatchManifestFile)}] Exception: {e}");
            }

            return false;
        }


    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniFan.Res
{
    public static class AssetBundleUtility
    {

        #region 编辑器使用相关
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            Debug.Log("Init->activeBundleMode: " + ActiveBundleMode);
        }

        // 编辑器开启真实 AssetBundle模式
        public static EditorPreferenceBool ActiveBundleMode { get; } = new EditorPreferenceBool(nameof(ActiveBundleMode));

        // 编辑器模拟 AssetBundle的异步加载
        public static EditorPreferenceBool SimulationAsyncLoad { get; } = new EditorPreferenceBool(nameof(SimulationAsyncLoad));

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        public static string GetPlatformForAssetBundles(BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.tvOS:
                    return "tvOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return string.Empty;
            }
        }
#endif

        #endregion

        #region AssetBundle 加密相关
        public static void SetAssetBundleDecryptKey(string assetbundleKey)
        {
            Type typeAssetBundle = typeof(AssetBundle);
            var methodSetAssetBundleDecryptKey = typeAssetBundle.GetMethod("SetAssetBundleDecryptKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (string.IsNullOrEmpty(assetbundleKey))
            {
                methodSetAssetBundleDecryptKey?.Invoke(null, new object[] { null });
            }
            else
            {
                methodSetAssetBundleDecryptKey?.Invoke(null, new object[] { assetbundleKey });
            }

        }

        private static readonly byte[] abKey = System.Text.Encoding.ASCII.GetBytes("8Yyq9yF7XmyxRyl3");


        public static string GetAssetBundleKey()
        {
            var tmpKey = new byte[abKey.Length];
            System.Array.Copy(abKey, tmpKey, abKey.Length);
            for (int i = 0; i < tmpKey.Length; i++)
            {
                tmpKey[i] ^= (byte)(abKey[i] ^ tmpKey[i]);
                tmpKey[i] ^= (byte)(i);
            }
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] MD5buffer = md5.ComputeHash(tmpKey);
            string result = "";
            for (int i = 0; i < 8; i++)
            {
                result += MD5buffer[i].ToString("x2");
            }
            return result;
        }

#if UNITY_EDITOR
        public static void SetAssetBundleEncryptKey(string assetbundleKey)
        {
            Type typeBuildPipeline = typeof(BuildPipeline);
            var methodSetAssetBundleEncryptKey = typeBuildPipeline.GetMethod("SetAssetBundleEncryptKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (string.IsNullOrEmpty(assetbundleKey))
            {
                methodSetAssetBundleEncryptKey?.Invoke(null, new object[] { null });
            }
            else
            {
                methodSetAssetBundleEncryptKey?.Invoke(null, new object[] { assetbundleKey });
            }
        }
#endif

        #endregion
    }

}
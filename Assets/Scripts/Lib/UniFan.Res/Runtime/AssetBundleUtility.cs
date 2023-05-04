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


        static int activeBundleMode = -1;
        const string kActiveBundleMode = "ActiveBundleMode";

        public static bool ActiveBundleMode
        {
            get
            {
                if (activeBundleMode == -1)
                    activeBundleMode = EditorPrefs.GetBool(kActiveBundleMode, false) ? 1 : 0;
                return activeBundleMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != activeBundleMode)
                {
                    activeBundleMode = newValue;
                    EditorPrefs.SetBool(kActiveBundleMode, value);
                }
            }
        }

        static int simulationAsyncLoad = -1;
        const string kSimulationAsyncLoad = "SimulationAsyncLoad";

        public static bool SimulationAsyncLoad
        {
            get
            {
                if (simulationAsyncLoad == -1)
                    simulationAsyncLoad = EditorPrefs.GetBool(kSimulationAsyncLoad, false) ? 1 : 0;
                return simulationAsyncLoad != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != simulationAsyncLoad)
                {
                    simulationAsyncLoad = newValue;
                    EditorPrefs.SetBool(kSimulationAsyncLoad, value);
                }
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
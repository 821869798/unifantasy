using UnityEditor;
using UnityEngine;

namespace Main
{
    public static class HybridCLRUtil
    {

        public static string CodeDllPath = "Res/99_Codes/";
        public static string AotFileListName = "AotGenericList.bytes";
        public const string AOTMetadataPath = "AOTMetadata";

        #region 编辑器使用相关
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            Debug.Log("Init->HybridCLREditActive status: " + HybridCLREditActive);
        }


        static int hybridCLREditActive = -1;
        const string kHybridCLREditActive = "hybridCLREditActive";

        public static bool HybridCLREditActive
        {
            get
            {
                if (hybridCLREditActive == -1)
                    hybridCLREditActive = EditorPrefs.GetBool(kHybridCLREditActive, false) ? 1 : 0;
                return hybridCLREditActive != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != hybridCLREditActive)
                {
                    hybridCLREditActive = newValue;
                    EditorPrefs.SetBool(kHybridCLREditActive, value);
                }
            }
        }
#endif
        #endregion


    }
}

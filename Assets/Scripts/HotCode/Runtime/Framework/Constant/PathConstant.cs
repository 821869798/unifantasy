using UniFan;
using UniFan.Res;

namespace HotCode.Framework
{
    public static class PathConstant
    {
        public static readonly string UIPrefabPath = "Res/02_UIPrefabs/";

        public static readonly string PackScenePath = "Res/01_Scenes/";

        public static string GetUIPrefabPath(string prefabName)
        {
            return UIPrefabPath + prefabName + ".prefab";
        }

        public static string GetABScenePath(string sceneName)
        {
            return PackScenePath + sceneName + ResPathConsts.AssetBundleExtension;
        }

    }
}

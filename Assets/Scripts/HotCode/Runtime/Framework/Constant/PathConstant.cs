using UniFan.Res;

namespace HotCode.Framework
{
    public static class PathConstant
    {
        public static readonly string UIPrefabPath = "Res/02_UIPrefabs/";

        public static readonly string AtlasSpritePath = "Res/03_AtlasClips/";

        public static readonly string PackScenePath = "Res/01_Scenes/";

        public static string GetUIPrefabPath(string prefabName)
        {
            return UIPrefabPath + prefabName + ".prefab";
        }

        public static string GetAtlasSpritePath(string atlasName, string spriteName)
        {
            //分步concat，优化GC
            return string.Concat(AtlasSpritePath, atlasName, "/" + spriteName + ".png");
        }


        public static string GetABScenePath(string sceneName)
        {
            return PackScenePath + sceneName + ResPathConsts.AssetBundleExtension;
        }

    }
}

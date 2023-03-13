
namespace UniFan
{
    public static class PathConsts
    {
        public static string PersistentDataPath => FilePathHelper.Instance.PersistentDataPath;

        //图集后缀名

        public const string SpriteAtlasExtension = ".spriteatlas";


        public const string ResPathPrefix = "Res/";

        public const string MediaPath = "media/";

        //预支体后缀名
        public const string PrefabExtension = ".prefab";
        
        //AssetBundle 后缀名
        public const string AssetBundleExtension = ".ab";


        //ab包存放的开头路径
        public const string AssetbundleLoadPath = "bundles/";

        //存储自定义格式的所有ab包信息文件
        public const string ResManifestFilePath = "resmainfest.ab";

        //存储自定义格式的ab依赖信息文件
        public const string ResManifestBinaryConfigName = "resmanifest.bytes";

        //图集所在目录
        public const string SpriteAtlasPathHead = "Res/SpriteAtlas/";

        //UI所在目录

        public const string UIPrefabPathHead = "Res/UIPrefabs/";

        //角色立绘和L2D所在目录

        public const string CharacterPicPathHead = "Res/Character/";

        //缓存目录
        public const string CachePath = "Cache/";


        //语言包所在的目录
        public const string LocalizaBytesPathHead = "Res/Language/";

        //ScriptableConfig 所在的目录
        public const string ScriptableConfigPathHead = "Res/ScriptableConfig/";

        //Res下Image 所在的目录
        public const string ResourcesImagePath = "Res/Images/";

        //二进制文件后缀名
        public const string BytesExtension = ".bytes";
    }
}
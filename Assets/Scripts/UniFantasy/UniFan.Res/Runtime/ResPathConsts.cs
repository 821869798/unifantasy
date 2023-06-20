using System;

namespace UniFan.Res
{
    public static class ResPathConsts
    {
        //AssetBundle 后缀名
        public const string AssetBundleExtension = ".ab";

        //ab包存放的开头路径
        public const string AssetbundleLoadPath = "bundles/";

        //存储自定义格式的所有ab包信息文件
        public const string ResManifestFilePath = "resmainfest.ab";

        //存储自定义格式的ab依赖信息文件
        public const string ResManifestBinaryConfigName = "resmanifest.bytes";

        //媒体文件（视频，音频）
        public const string MediaPath = "media/";
    }
}

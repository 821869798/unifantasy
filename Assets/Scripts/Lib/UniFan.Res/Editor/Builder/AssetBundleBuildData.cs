using System;
using System.Collections.Generic;

namespace UniFan.ResEditor
{
    public class AssetBundleBuildData
    {
        //是否是公共依赖的ab包
        public bool isCommonAssetBundle = false;
        //ab包的名字
        public string assetBundleName;
        //原始匹配项的资源
        public HashSet<string> originAssetNames = new HashSet<string>();
        //所有的资源名
        public HashSet<string> assetNames = new HashSet<string>();
        //直接依赖的ab包的名字
        public List<string> dependencies = new List<string>();
        //短名字,用于存储
        public List<string> shortAssetNames = new List<string>();
        //ab的Hash值
        public string hashValue;
        //不写入包含的文件到Manifest中，例如lua脚本、图集
        public ManifestWriteType manifestWriteType = ManifestWriteType.WriteAll;
    }
}

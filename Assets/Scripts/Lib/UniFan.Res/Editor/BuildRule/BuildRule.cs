using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UniFan.Res.Editor
{
    public enum RulePackerType
    {
        FileName = 0,               //每个文件单独一个ab包
        DirectoryName = 1,          //每个文件夹一个ab包
        AssetBundleName = 2,        //所有文件一个ab包
        TobSubDirectoryName = 3,    //顶层的所有子文件各自一个ab包
    }

    public enum ManifestWriteType
    {
        WriteAll = 0,               //写入所有信息
        NoWriteContianFile = 1,     //不写入包含的信息
        OnlyWritePatternFile = 2,   //只写入匹配到的文件
    }

    //引用在某个语言下剔除的情况
    public enum BuildCullingLangType
    {
        ZH_CN = 1 << 0,
        ZH_TW = 1 << 1,
        EN_US = 1 << 2,
        JA_JP = 1 << 3,
        KO_KR = 1 << 4,
    }

    [System.Serializable]
    public class BuildRule
    {
        //是否开启该规则
        public bool active = true;
        //该打包配置描述
        public string buildDesc;
        //打包类型
        public RulePackerType buildType = RulePackerType.FileName;
        //是否覆盖AssetBundleName模式的Bundle名字
        public bool isOverrideBundleName = false;
        //覆盖的Bundle名字
        public string overrideBundleName;
        //资源搜索目录
        public string searchPath;
        //文件搜索通配符
        public string searchPattern;
        //文件搜索选项，递归下层目录或者仅当前层路径
        public SearchOption searchOption = SearchOption.AllDirectories;
        //是否强制包含依赖文件到本ab包中,例如图集把散图打进本包管理
        public bool forceInclueDeps = false;
        //不写入包含的文件到Manifest中，例如lua脚本、图集
        public ManifestWriteType manifestWriteType = ManifestWriteType.WriteAll;
        //该包在某种语言环境下不被引用
        public int depCulling = 0;
        //忽略这种引用规则
        public bool ignoreDepCulling;
    }

}

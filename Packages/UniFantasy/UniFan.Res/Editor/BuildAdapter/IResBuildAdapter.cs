using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFan.ResEditor
{
    public interface IResBuildAdapter
    {

        public eLanguageType languageType { get; }

        /// <summary>
        /// 是否是打当前语言的资源
        /// </summary>
        bool onlyBuildCurLang { get; }

        /// <summary>
        /// 是否加密ab
        /// </summary>
        bool isABEncrypt { get; }

        /// <summary>
        /// 是否是增量打包模式
        /// </summary>
        bool isIncrement { get; }

        /// <summary>
        /// 开始打包ab
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="builds"></param>
        /// <param name="targetPlatform"></param>
        /// <returns></returns>
        bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildTarget targetPlatform);


        /// <summary>
        /// 获取ab的hash
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        Hash128 GetAssetBundleHash(string assetBundleName);


        /// <summary>
        /// 获取ab包的直接依赖
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        string[] GetDirectDependencies(string assetBundleName);

    }
}

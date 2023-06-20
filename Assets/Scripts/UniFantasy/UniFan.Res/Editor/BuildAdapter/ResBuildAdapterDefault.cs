using System;
using UniFan;
using UniFan.Res;
using UnityEditor;
using UnityEngine;

namespace UniFan.ResEditor
{
    public class ResBuildAdapterDefault : IResBuildAdapter
    {
        public virtual eLanguageType languageType { set; get; }

        public virtual bool onlyBuildCurLang { set; get; } = true;

        public virtual bool isABEncrypt { set; get; } = false;

        public virtual bool isIncrement { set; get; }

        protected AssetBundleManifest _abManifest;


        public ResBuildAdapterDefault(eLanguageType language, bool isIncrement)
        {
            this.languageType = language;
            this.isIncrement = isIncrement;
        }

        protected virtual BuildAssetBundleOptions GetAssetBundleOptions()
        {
            var options = BuildAssetBundleOptions.None;

            options |= BuildAssetBundleOptions.ChunkBasedCompression;

            if (!this.isIncrement)
            {
                //非增量打包方式
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            // 资源不变的情况下每次打包都一致
            options |= BuildAssetBundleOptions.DeterministicAssetBundle;

            //设置AssetBundle加密
            if (this.isABEncrypt)
            {
                Type typeBuildAssetBundleOptions = typeof(BuildAssetBundleOptions);
                var fieldEnableProtection = typeBuildAssetBundleOptions.GetField("EnableProtection");
                if (fieldEnableProtection != null)
                {
                    var valueEnableProtection = System.Convert.ToInt32(fieldEnableProtection.GetValue(null));
                    options |= (BuildAssetBundleOptions)valueEnableProtection;
                }
                AssetBundleUtility.SetAssetBundleEncryptKey(AssetBundleUtility.GetAssetBundleKey());
            }
            else
            {
                AssetBundleUtility.SetAssetBundleEncryptKey(null);
            }

            return options;
        }


        public virtual bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildTarget targetPlatform)
        {
            var options = GetAssetBundleOptions();

            var manifest = BuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            _abManifest = manifest;
            return _abManifest != null;
        }

        public virtual Hash128 GetAssetBundleHash(string assetBundleName)
        {
            return _abManifest.GetAssetBundleHash(assetBundleName);
        }

        public virtual string[] GetDirectDependencies(string assetBundleName)
        {
            return _abManifest.GetDirectDependencies(assetBundleName);
        }
    }
}

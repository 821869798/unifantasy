using System;
using UniFan;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace UniFan.ResEditor
{
    public class ResBuildAdapterSBP : ResBuildAdapterDefault
    {
        public ResBuildAdapterSBP(eLanguageType language, bool isIncrement) : base(language, isIncrement)
        {
        }

        private CompatibilityAssetBundleManifest _sbpManifest;


        public override bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildTarget targetPlatform)
        {
            var options = GetAssetBundleOptions();

            var manifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            _sbpManifest = manifest;
            return _sbpManifest != null;
        }

        public override Hash128 GetAssetBundleHash(string assetBundleName)
        {
            return _sbpManifest.GetAssetBundleHash(assetBundleName);
        }

        public override string[] GetDirectDependencies(string assetBundleName)
        {
            return _sbpManifest.GetDirectDependencies(assetBundleName);
        }

    }
}

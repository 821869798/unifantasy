using System.Collections.Generic;

namespace UniFan.Res.Editor
{
    internal class RulePackerByAssetBundleName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            if (ABBuildCreator.ShowRulePackerProgressBar(rule.searchPath, 1, 1))
            {
                return false;
            }

            var files = ABBuildCreator.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            if (files.Count == 0)
            {
                return true;
            }
            List<string> assetNames = new List<string>();
            if (rule.forceInclueDeps)
            {
                foreach (var item in files)
                {
                    assetNames.AddRange(ABBuildCreator.GetAssetDependencies(item));
                }
            }
            assetNames.AddRange(files);


            string assetBundleName;
            if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
            {
                assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(rule.overrideBundleName);
            }
            else
            {
                assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(rule.searchPath);
            }

            var buildData = ABBuildCreator.TryNewBuildData(assetBundleName, rule);
            buildData.originAssetNames.AddRange(files);
            if (!ABBuildUtility.CheckAssetBundleName(buildData.assetBundleName))
            {
                return false;
            }
            buildData.assetNames.AddRange(assetNames);
            ABBuildCreator.AddPackedAssets(assetNames);
            buildData.manifestWriteType = rule.manifestWriteType;
            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            string assetBundleName = string.Empty;
            if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
            {
                assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(rule.overrideBundleName, true);
            }
            else
            {
                assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(rule.searchPath, true);
            }
            return assetBundleName;
        }
    }
}

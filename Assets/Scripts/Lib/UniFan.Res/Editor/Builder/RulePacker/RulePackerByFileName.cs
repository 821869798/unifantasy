using UnityEngine;
using UniFan.Res.Editor;

namespace Lib.UniFan.Res.Editor
{
    internal class RulePackerByFileName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            var files = RulePackerUtility.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (ABBuildCreator.ShowRulePackerProgressBar(item, i, files.Count))
                {
                    return false;
                }

                AssetBundleBuildData buildData = new AssetBundleBuildData();
                buildData.assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(item);
                if (!RulePackerUtility.CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                if (rule.forceInclueDeps)
                {
                    buildData.assetNames.AddRange(RulePackerUtility.GetAssetDependencies(item));
                }
                buildData.assetNames.Add(item);
                buildData.originAssetNames.Add(item);
                buildData.manifestWriteType = rule.manifestWriteType;

                ABBuildCreator.AddPackedAssets(buildData.assetNames);
                ABBuildCreator.AddBuildData(buildData, rule);
            }
            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            Debug.LogError($"{nameof(RulePackerByFileName)} no support ShareRule");
            return string.Empty;
        }
    }
}

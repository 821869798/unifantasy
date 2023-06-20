using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UniFan.ResEditor
{
    internal class RulePackerByFileName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            var files = ABBuildCreator.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (ABBuildCreator.ShowRulePackerProgressBar(item, i, files.Count))
                {
                    return false;
                }

                var assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(item);
                var buildData = ABBuildCreator.TryNewBuildData(assetBundleName, rule);
                if (!ABBuildUtility.CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                if (rule.forceInclueDeps)
                {
                    buildData.assetNames.AddRange(ABBuildCreator.GetAssetDependencies(item));
                }
                buildData.assetNames.Add(item);
                buildData.originAssetNames.Add(item);
                ABBuildCreator.AddPackedAssets(buildData.assetNames);
                buildData.manifestWriteType = rule.manifestWriteType;
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

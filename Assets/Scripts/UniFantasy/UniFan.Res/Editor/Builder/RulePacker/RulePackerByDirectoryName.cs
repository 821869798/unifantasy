using System.Collections.Generic;
using System.IO;

namespace UniFan.ResEditor
{
    internal class RulePackerByDirectoryName : IRulePacker
    {

        public bool ResRulePacker(BuildRule rule)
        {
            var files = ABBuildCreator.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption, rule.searchRegex);

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> originBundleAssets = new Dictionary<string, List<string>>();

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var path = Path.GetDirectoryName(item);
                if (!bundles.ContainsKey(path))
                {
                    bundles[path] = new List<string>();
                    originBundleAssets[path] = new List<string>();
                }
                bundles[path].Add(item);
                originBundleAssets[path].Add(item);
                if (rule.forceInclueDeps)
                {
                    bundles[path].AddRange(ABBuildCreator.GetAssetDependencies(item));
                }
            }
            int count = 0;
            foreach (var item in bundles)
            {
                count++;

                if (ABBuildCreator.ShowRulePackerProgressBar(item.Key, count, bundles.Count))
                {
                    return false;
                }

                var assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(item.Key);
                AssetBundleBuildData buildData = ABBuildCreator.TryNewBuildData(assetBundleName, rule);
                if (!ABBuildUtility.CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                buildData.assetNames.AddRange(item.Value);
                buildData.originAssetNames.AddRange(originBundleAssets[item.Key]);
                ABBuildCreator.AddPackedAssets(item.Value);
                if (rule.ignoreAssetGuid)
                {
                    ABBuildCreator.ReplaceAssetGuidDeterministic(item.Value);
                }
                buildData.manifestWriteType = rule.manifestWriteType;
            }
            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            string assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(shareAssetName), true);
            return assetBundleName;
        }
    }
}

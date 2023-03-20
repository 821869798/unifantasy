using System.Collections.Generic;
using System.IO;

namespace UniFan.Res.Editor
{
    internal class RulePackerByDirectoryName : IRulePacker
    {

        public bool ResRulePacker(BuildRule rule)
        {
            var files = RulePackerUtility.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);

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
                    bundles[path].AddRange(RulePackerUtility.GetAssetDependencies(item));
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

                AssetBundleBuildData buildData = new AssetBundleBuildData();
                buildData.assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(item.Key);
                if (!RulePackerUtility.CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                buildData.assetNames.AddRange(item.Value);
                buildData.originAssetNames.AddRange(originBundleAssets[item.Key]);
                buildData.manifestWriteType = rule.manifestWriteType;

                ABBuildCreator.AddPackedAssets(item.Value);
                ABBuildCreator.AddBuildData(buildData, rule);
            }
            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            string assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(shareAssetName), true);
            return assetBundleName;
        }
    }
}

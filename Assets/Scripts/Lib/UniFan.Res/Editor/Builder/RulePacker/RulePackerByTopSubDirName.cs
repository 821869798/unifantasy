using System.Collections.Generic;
using System.IO;


namespace UniFan.Res.Editor
{
    internal class RulePackerByTopSubDirName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            var files = ABBuildCreator.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> originBundleAssets = new Dictionary<string, List<string>>();


            int searchPathLength = rule.searchPath.Length;
            if (!rule.searchPath.EndsWith("/"))
            {
                searchPathLength++;
            }

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];

                var backSection = item.Substring(searchPathLength);
                var firstSep = backSection.IndexOf('/');

                string topSubPath;
                if (firstSep >= 0)
                {
                    topSubPath = item.Substring(0, searchPathLength + firstSep);
                }
                else
                {
                    topSubPath = Path.GetDirectoryName(item);
                }

                if (!bundles.ContainsKey(topSubPath))
                {
                    bundles[topSubPath] = new List<string>();
                    originBundleAssets[topSubPath] = new List<string>();
                }
                bundles[topSubPath].Add(item);
                originBundleAssets[topSubPath].Add(item);
                if (rule.forceInclueDeps)
                {
                    bundles[topSubPath].AddRange(ABBuildCreator.GetAssetDependencies(item));
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
                buildData.assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(item.Key);
                if (!ABBuildUtility.CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                buildData.assetNames.AddRange(item.Value);
                buildData.originAssetNames.AddRange(originBundleAssets[item.Key]);
                ABBuildCreator.AddPackedAssets(item.Value);
                buildData.manifestWriteType = rule.manifestWriteType;
            }
            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            int searchPathLength = rule.searchPath.Length;
            if (!rule.searchPath.EndsWith("/"))
            {
                searchPathLength++;
            }
            var backSection = shareAssetName.Substring(searchPathLength);
            var firstSep = backSection.IndexOf('/');

            string topSubPath;
            if (firstSep >= 0)
            {
                topSubPath = shareAssetName.Substring(0, searchPathLength + firstSep);
            }
            else
            {
                topSubPath = Path.GetDirectoryName(shareAssetName);
            }

            string assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(topSubPath, true);
            return assetBundleName;
        }


    }
}

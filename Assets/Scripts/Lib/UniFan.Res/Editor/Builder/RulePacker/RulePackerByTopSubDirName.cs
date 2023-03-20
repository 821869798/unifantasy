using System;
using System.Collections.Generic;
using System.IO;
using static PlasticGui.Help.GuiHelp;
using static UnityEditor.Progress;

namespace UniFan.Res.Editor
{
    internal class RulePackerByTopSubDirName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            var files = RulePackerUtility.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> originBundleAssets = new Dictionary<string, List<string>>();


            int searchPathLength = rule.searchPath.Length;
            if(!rule.searchPath.EndsWith("/"))
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
                    bundles[topSubPath].AddRange(RulePackerUtility.GetAssetDependencies(item));
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

            string assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(topSubPath, true);
            return assetBundleName;
        }


    }
}

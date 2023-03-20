using System.Collections.Generic;
using System.IO;
using static PlasticGui.Help.GuiHelp;
using UnityEditor.Build.Reporting;
using static UnityEditor.Progress;
using Lib.UniFan.Res.Editor;

namespace UniFan.Res.Editor
{
    internal class RulePackerByAssetBundleName : IRulePacker
    {
        public bool ResRulePacker(BuildRule rule)
        {
            if (ABBuildCreator.ShowRulePackerProgressBar(rule.searchPath,1,1))
            {
                return false;
            }

            var files = RulePackerUtility.GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            if (files.Count == 0)
            {
                return true;
            }
            List<string> assetNames = new List<string>();
            if (rule.forceInclueDeps)
            {
                foreach (var item in files)
                {
                    assetNames.AddRange(RulePackerUtility.GetAssetDependencies(item));
                }
            }
            assetNames.AddRange(files);


            AssetBundleBuildData buildData = new AssetBundleBuildData();
            buildData.originAssetNames.AddRange(files);
            if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
            {
                buildData.assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(rule.overrideBundleName);
            }
            else
            {
                buildData.assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(rule.searchPath);
            }
            if (!RulePackerUtility.CheckAssetBundleName(buildData.assetBundleName))
            {
                return false;
            }
            buildData.assetNames.AddRange(assetNames);
            buildData.manifestWriteType = rule.manifestWriteType;

            ABBuildCreator.AddPackedAssets(assetNames);
            ABBuildCreator.AddBuildData(buildData, rule);

            return true;
        }

        public string GetShareRulePackerName(BuildRule rule, string shareAssetName)
        {
            string assetBundleName = string.Empty;
            if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
            {
                assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(rule.overrideBundleName, true);
            }
            else
            {
                assetBundleName = RulePackerUtility.BuildAssetBundleNameWithAssetPath(rule.searchPath, true);
            }
            return assetBundleName;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UniFan.Res.Editor
{
    public class AssetBundleBuildConfig : ScriptableObject
    {
        public List<BuildRule> buildRules = new List<BuildRule>();

        public List<BuildRule> sharedBuildRules = new List<BuildRule>();

        public static AssetBundleBuildConfig LoadOrCreateConfig()
        {

            AssetBundleBuildConfig configAssets = AssetDatabase.LoadAssetAtPath<AssetBundleBuildConfig>(Consts.BuildConfigPath);
            if (configAssets != null)
            {
                return configAssets;
            }
            string parentDir = Path.GetDirectoryName(Consts.BuildConfigPath);
            if (Directory.Exists(parentDir) == false)
            {
                Directory.CreateDirectory(parentDir);
            }
            configAssets = ScriptableObject.CreateInstance<AssetBundleBuildConfig>();
            AssetDatabase.CreateAsset(configAssets, Consts.BuildConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return configAssets;
        }
    }
}

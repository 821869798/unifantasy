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
            AssetBundleBuildConfig configAssets = EditorHelper.LoadSettingData<AssetBundleBuildConfig>();
            return configAssets;
        }
    }
}

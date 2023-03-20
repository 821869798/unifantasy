using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;

namespace UniFan.Res.Editor
{
    [CreateAssetMenu(fileName = "BundleBuildFilterConfig", menuName = "Create Scriptable Config/打包配置/AssetBundle打包筛选配置")]
    public class BuildFilterConfig : SerializedScriptableObject
    {
        //[SerializeField]
        //[ShowInInspector]
        //public PathTrieTree buildFilterData = new PathTrieTree();

        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "路径名", ValueLabel = "筛选类型")]
        public Dictionary<string, PathTrieTree.PathValueType> buildFilterData = new Dictionary<string, PathTrieTree.PathValueType>();


        /// <summary>
        /// 获取字典树格式的配置数据
        /// </summary>
        /// <returns></returns>
        public PathTrieTree GetTrieFilterData()
        {
            PathTrieTree trieData = new PathTrieTree();
            foreach (var filter in buildFilterData)
            {
                trieData.AddPath(filter.Key, filter.Value);
            }
            return trieData;
        }


        private static BuildFilterConfig _globalBuildFilterConfig;
        public static BuildFilterConfig GlobalBuildFilterConfig
        {
            get
            {
                if (_globalBuildFilterConfig == null)
                {
                    ReloadConfig();
                }
                return _globalBuildFilterConfig;
            }
        }

        private static void ReloadConfig()
        {
            _globalBuildFilterConfig = EditorHelper.LoadSettingData<BuildFilterConfig>();
        }

    }

}


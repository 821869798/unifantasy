using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniFan.Res.Editor
{

    public class BuildFilterEditorMenu
    {

        #region BuildFilter

        [MenuItem("Assets/GameEditor/资源打包筛选/选中添加到黑名单")]
        static void SelectAddToBlackList()
        {
            var selectGuids = Selection.assetGUIDs;
            foreach (var guid in selectGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.TryGetValue(assetPath, out var valueType))
                {
                    BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.Add(assetPath, PathTrieTree.PathValueType.BlackList);
                }
                else
                {
                    BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData[assetPath] = PathTrieTree.PathValueType.BlackList;
                }

                Debug.Log($"[打包黑名单添加]{assetPath}");
            }
            EditorUtility.SetDirty(BuildFilterConfig.GlobalBuildFilterConfig);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/GameEditor/资源打包筛选/选中添加到白名单")]
        static void SelectAddToWhiteList()
        {
            var selectGuids = Selection.assetGUIDs;
            foreach (var guid in selectGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.TryGetValue(assetPath, out var valueType))
                {
                    BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.Add(assetPath, PathTrieTree.PathValueType.WhiteList);
                }
                else
                {
                    BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData[assetPath] = PathTrieTree.PathValueType.WhiteList;
                }
                Debug.Log($"[打包白名单添加]{assetPath}");
            }
            EditorUtility.SetDirty(BuildFilterConfig.GlobalBuildFilterConfig);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/GameEditor/资源打包筛选/从筛选名单中移除")]
        static void SelectRemoveFromConfig()
        {
            var selectGuids = Selection.assetGUIDs;
            if (selectGuids == null || selectGuids.Length <= 0)
            {
                return;
            }
            var trieData = BuildFilterConfig.GlobalBuildFilterConfig.GetTrieFilterData();
            foreach (var guid in selectGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var node = trieData.GetPath(assetPath);
                if (node.childCount > 0 && EditorUtility.DisplayDialog("警告", "改筛选配置节点下有其他子节点，是否删除所有子节点", "包括子节点都删除", "只删除当前"))
                {
                    var nodeList = node.GetChildNodePathList();
                    foreach (var nodeValue in nodeList)
                    {
                        string path = assetPath + nodeValue;
                        if (BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.Remove(path))
                            Debug.Log($"[删除筛选名单<{node.nodeType}>]{path}");
                    }
                }
                if (BuildFilterConfig.GlobalBuildFilterConfig.buildFilterData.Remove(assetPath))
                    Debug.Log($"[删除筛选名单<{node.nodeType}>]{assetPath}");
            }
            EditorUtility.SetDirty(BuildFilterConfig.GlobalBuildFilterConfig);
            AssetDatabase.SaveAssets();
        }

        #endregion
    }

}
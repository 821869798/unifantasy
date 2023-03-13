using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniFan.Res.Editor
{

    [System.Serializable]
    public class PathTrieTree
    {
        [System.Serializable]
        public enum PathValueType
        {
            None = 0,           //无状态
            BlackList = 1,      //黑名单
            WhiteList = 2,      //白名单
            OverseasCulling = 3, //海外服屏蔽资源
        }

        public enum OverseasChannelCullingType
        {
            En = 1,                  //美服
            Jp = 2,                  //日服
            Kr = 3,                  //韩服
            Tw = 4,                  //台服
        }

        [System.Serializable]
        public class PathTrieNode
        {
            public string nodeValue;
            public PathValueType nodeType = PathValueType.None;
            public int overseasType = 0;
            //[SerializeField]
            //[ShowInInspector]
            //[DictionaryDrawerSettings(KeyLabel = "路径名", ValueLabel = "配置")]
            /// <summary>
            /// 所有的子节点
            /// </summary>
            public Dictionary<string, PathTrieNode> children = new Dictionary<string, PathTrieNode>();

            public int childCount => children.Count;

            public PathTrieNode(string nodeValue)
            {
                this.nodeValue = nodeValue;
            }

            public PathTrieNode(string nodeValue, PathValueType nodeType)
            {
                this.nodeValue = nodeValue;
                this.nodeType = nodeType;
            }

            public PathTrieNode AddChildNode(string nodeValue)
            {
                if (children.TryGetValue(nodeValue, out var node))
                {
                    return node;
                }
                node = new PathTrieNode(nodeValue);
                children.Add(nodeValue, node);
                return node;
            }

            public PathTrieNode GetChildNode(string nodeValue)
            {
                if (children.TryGetValue(nodeValue, out var node))
                {
                    return node;
                }
                return null;
            }

            public bool RemoveChildNode(string nodeValue)
            {
                return children.Remove(nodeValue);
            }

            public List<string> GetChildNodePathList()
            {
                List<string> nodePaths = new List<string>();
                this.GetChildNodePathListInternal(string.Empty, nodePaths);
                return nodePaths;
            }

            private void GetChildNodePathListInternal(string prePath, List<string> nodePaths)
            {
                foreach (var node in children)
                {
                    string path = prePath + PathTrieTree.separator + node.Key;
                    if (node.Value.childCount <= 0 || node.Value.nodeType != PathValueType.None)
                    {
                        nodePaths.Add(path);
                    }

                    if (node.Value.childCount > 0)
                    {
                        node.Value.GetChildNodePathListInternal(path, nodePaths);
                    }
                }
            }
            internal void Clear()
            {
                children.Clear();
            }

        }

        [SerializeField]
        private PathTrieNode rootTrieNode;

        //分隔符
        public const char separator = '/';

        public PathTrieTree()
        {
            rootTrieNode = new PathTrieNode(string.Empty);
        }

        public void AddPath(string path, PathValueType nodeType, int overseasType = 0)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var pathNodes = path.Split(separator);
            var p = rootTrieNode;
            for (int i = 0; i < pathNodes.Length; i++)
            {
                var nodeValue = pathNodes[i];
                var node = p.AddChildNode(nodeValue);
                p = node;
            }
            if (p != rootTrieNode)
            {
                p.nodeType = nodeType;
                p.overseasType = overseasType;
            }

        }

        public PathTrieNode GetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var pathNodes = path.Split(separator);
            var p = rootTrieNode;
            for (int i = 0; i < pathNodes.Length; i++)
            {
                var nodeValue = pathNodes[i];
                var node = p.GetChildNode(nodeValue);
                if (node == null)
                {
                    break;
                }
                p = node;
            }
            if (p == rootTrieNode)
            {
                return null;
            }
            return p;
        }

        public bool RemovePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            var pathNodes = path.Split(separator);
            var p = rootTrieNode;
            PathTrieNode lastP = p;
            for (int i = 0; i < pathNodes.Length; i++)
            {
                var nodeValue = pathNodes[i];
                var node = p.GetChildNode(nodeValue);
                lastP = p;
                p = node;
            }
            if (p == rootTrieNode)
            {
                return false;
            }
            return lastP.RemoveChildNode(p.nodeValue);
        }

        public PathValueType GetPathValueType(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return PathValueType.None;
            }
            var pathNodes = path.Split(separator);
            PathValueType lastType = PathValueType.None;
            var p = rootTrieNode;
            for (int i = 0; i < pathNodes.Length; i++)
            {
                var nodeValue = pathNodes[i];
                var node = p.GetChildNode(nodeValue);
                if (node == null)
                {
                    break;
                }
                if (node.nodeType != PathValueType.None)
                {
                    lastType = node.nodeType;
                }
                p = node;
            }
            return lastType;
        }

    }

    //public class PathTrieTreeDrawer : OdinValueDrawer<PathTrieTree>
    //{
    //    protected override void DrawPropertyLayout(GUIContent label)
    //    {
    //        SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), Color.blue);
    //        this.CallNextDrawer(label);
    //    }
    //}

    //public class PathTrieNodeDrawer : OdinValueDrawer<PathTrieTree.PathTrieNode>
    //{
    //    protected override void DrawPropertyLayout(GUIContent label)
    //    {
    //        //SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), Color.blue);
    //        this.CallNextDrawer(label);
    //    }
    //}

}

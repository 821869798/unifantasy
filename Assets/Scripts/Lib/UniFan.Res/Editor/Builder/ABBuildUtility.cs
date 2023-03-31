using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UniFan.Res.Editor
{
    internal static class ABBuildUtility
    {
        public static readonly string ProjectPath = Path.GetDirectoryName(Application.dataPath);

        public static List<string> GetFilesWithoutDirectories(string searchPath, string searchPattern, SearchOption searchOption)
        {

            List<string> items = new List<string>();
            if (!Directory.Exists(searchPath))
            {
                return items;
            }
            var files = Directory.GetFiles(searchPath, searchPattern, searchOption);
            foreach (var item in files)
            {
                var assetPath = item.Replace('\\', '/');
                if (!Directory.Exists(assetPath))
                {
                    items.Add(assetPath);
                }
            }
            return items;
        }

        public static string BuildAssetBundleNameWithAssetPath(string assetPath, bool isShared = false)
        {
            string fullPath = Path.Combine(ProjectPath, assetPath);
            string fileName;
            if (Directory.Exists(fullPath))
            {
                fileName = Path.GetFileName(assetPath);
            }
            else
            {
                fileName = Path.GetFileNameWithoutExtension(assetPath);
            }

            if (isShared)
            {
                fileName = Consts.CommonAssetBundlePrefix + fileName;
            }
            string name = (Path.Combine(Path.GetDirectoryName(assetPath), fileName) + Consts.BundleExtensionName).Replace('\\', '/').ToLower();
            if (name.StartsWith("assets/"))
            {
                name = name.Substring(7);
            }
            return name;
        }


        public static int GetSceneAssetCount(HashSet<string> buildAssets)
        {
            int count = 0;
            foreach (var item in buildAssets)
            {
                if (item.EndsWith(".unity"))
                {
                    count++;
                }
            }
            return count;
        }


        /// <summary>
        /// 检车AssetBundle名字的合法性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckAssetBundleName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if ((int)name[i] > 127)
                {
                    Debug.LogError("[Error] Asset Bundle is illegal,name:" + name);
                    return false;
                }
            }
            return true;
        }



        public static void AddRange<T>(this HashSet<T> setMap, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!setMap.Contains(list[i]))
                    setMap.Add(list[i]);
            }
        }

        public static void AddRange<T>(this HashSet<T> setMap, HashSet<T> otherSet)
        {
            foreach (var item in otherSet)
            {
                if (!setMap.Contains(item))
                {
                    setMap.Add(item);
                }
            }
        }

        public static T[] ToArray<T>(this HashSet<T> setMap)
        {
            T[] array = new T[setMap.Count];
            int i = 0;
            foreach (var item in setMap)
            {
                array[i++] = item;
            }
            return array;
        }

    }
}

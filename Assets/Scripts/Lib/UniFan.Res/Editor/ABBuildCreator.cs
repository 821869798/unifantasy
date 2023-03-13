using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniFan.Res.Editor
{
    public static class ABBuildCreator
    {
        public static eLanguageType buildLang = eLanguageType.ZH_CN;
        public static bool onlyBuildCurLang = false;        //只打包当前语言的资源
        public static Regex buildLangRegex;
        public static BuildCullingLangType cullingLangType = BuildCullingLangType.ZH_CN;
        public static List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        public static List<BuildRule> rules = new List<BuildRule>();
        public static List<BuildRule> shareRules = new List<BuildRule>();
        public static HashSet<string> packedAssets = new HashSet<string>();
        public static Dictionary<string, HashSet<string>> allDependencies = new Dictionary<string, HashSet<string>>();
        public static List<AssetBundleBuildData> buildDatas = new List<AssetBundleBuildData>();
        public static HashSet<string> depCullingBundles = new HashSet<string>();        //需要剔除依赖的
        public static HashSet<string> depCullingIgnoreBundles = new HashSet<string>();  //忽略剔除规则的包
        public static readonly string ProjectPath = Path.GetDirectoryName(Application.dataPath);
        //筛选字典树
        private static PathTrieTree trieTreeFilter;

        //海外渠道剔除资源筛选树
        private static PathTrieTree overseasCullingFilter;

        public static int CurRuleIndex;

        public static void ResetData()
        {
            builds.Clear();
            rules.Clear();
            shareRules.Clear();
            packedAssets.Clear();
            allDependencies.Clear();
            buildDatas.Clear();
            depCullingBundles.Clear();
            depCullingIgnoreBundles.Clear();
            CurRuleIndex = 0;
            trieTreeFilter = BuildFilterConfig.GlobalBuildFilterConfig.GetTrieFilterData();
        }

        public static bool GetBuilds(eLanguageType lang, bool buildlang, out List<AssetBundleBuild> outBuilds)
        {
            outBuilds = null;

            ResetData();

            buildLang = lang;
            cullingLangType = Consts.LangToCullingType(lang);
            onlyBuildCurLang = buildlang;

            //打包语言正则
            if(onlyBuildCurLang)
            {
                buildLangRegex = GetBuildLangRegex(lang);
            }

            AssetBundleBuildConfig buildConfig = AssetBundleBuildConfig.LoadOrCreateConfig();
            foreach (var rule in buildConfig.buildRules)
            {
                if (rule.active)
                {
                    rules.Add(rule);
                }
            }
            foreach (var rule in buildConfig.sharedBuildRules)
            {
                if (rule.active && rule.buildType != RuleBuilType.FileName)
                {
                    shareRules.Add(rule);
                }
            }

            for (int i = 0; i < rules.Count; i++)
            {
                CurRuleIndex = i + 1;
                var item = rules[i];
                if (!GenRuleBuild(item))
                {
                    Debug.LogError($"rule get build failed:{item.searchPath} {item.searchPattern}");
                    return false;
                }
            }
            //收集资源的依赖
            CollectDependencies();

            //如果被依赖的数量大于1，说明是公共资源，需要单独打包
            if (!GenDependenciesBuild())
            {
                return false;
            }

            //生成最终的BuildList
            for (int i = 0; i < buildDatas.Count; i++)
            {
                if (!GenBuildList(buildDatas[i]))
                {
                    return false;
                }
            }
            outBuilds = builds;
            return true;
        }

        public static Regex GetBuildLangRegex(eLanguageType curlang)
        {
            var langKey = curlang.ToString();
            var langNames = System.Enum.GetNames(typeof(eLanguageType));
            string rex = string.Empty;
            foreach (var name in langNames)
            {
                if (name != langKey)
                {
                    if (string.IsNullOrEmpty(rex))
                    {
                        rex += name;
                    }
                    else
                    {
                        rex += "|" + name;
                    }
                }
            }
            string regexStr = ".*?(" + rex + ").*?";
            return new Regex(regexStr, RegexOptions.IgnoreCase);
        }

        public static bool GenRuleBuild(BuildRule rule)
        {
            switch (rule.buildType)
            {
                case RuleBuilType.FileName:
                    return GenRuleBuildByFileName(rule);
                case RuleBuilType.DirectoryName:
                    return GenRuleBuildByDirectoryName(rule);
                case RuleBuilType.AssetBundleName:
                    return GenRuleBuildByAssetBundleName(rule);
            }
            return false;
        }

        private static void AddBuildData(AssetBundleBuildData buildData,BuildRule rule = null)
        {
            buildDatas.Add(buildData);

            if(rule == null)
            {
                return;
            }    
            if ( ((int)cullingLangType & rule.depCulling) > 0)
            {
                //需要剔除引用的
                depCullingBundles.Add(buildData.assetBundleName);
            }
            if(rule.ignoreDepCulling)
            {
                //忽略引用剔除规则的包
                depCullingIgnoreBundles.Add(buildData.assetBundleName);
            }
        }

        public static bool GenRuleBuildByFileName(BuildRule rule)
        {
            var files = GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing index At({0}/{1})... progress:[{2}/{3}]", CurRuleIndex, rules.Count, i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }

                AssetBundleBuildData buildData = new AssetBundleBuildData();
                buildData.assetBundleName = BuildAssetBundleNameWithAssetPath(item);
                if (!CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                if (rule.forceInclueDeps)
                {
                    buildData.assetNames.AddRange(GetAssetDependencies(item));
                }
                buildData.assetNames.Add(item);
                buildData.originAssetNames.Add(item);
                packedAssets.AddRange(buildData.assetNames);
                buildData.manifestWriteType = rule.manifestWriteType;
                AddBuildData(buildData, rule);
            }
            return true;
        }

        public static bool GenRuleBuildByDirectoryName(BuildRule rule)
        {
            var files = GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> originBundleAssets = new Dictionary<string, List<string>>();

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                UnityEditor.EditorUtility.DisplayProgressBar(string.Format("Collecting index At({0}/{1})... progress:[{2}/{3}]", CurRuleIndex, rules.Count, i, files.Count), item, i * 1f / files.Count);
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
                    bundles[path].AddRange(GetAssetDependencies(item));
                }
            }
            int count = 0;
            foreach (var item in bundles)
            {
                count++;
                UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing index At({0}/{1})... progress:[{2}/{3}]", CurRuleIndex, rules.Count, count, bundles.Count), item.Key, count * 1f / bundles.Count);
                AssetBundleBuildData buildData = new AssetBundleBuildData();
                buildData.assetBundleName = BuildAssetBundleNameWithAssetPath(item.Key);
                if (!CheckAssetBundleName(buildData.assetBundleName))
                {
                    return false;
                }
                buildData.assetNames.AddRange(item.Value);
                buildData.originAssetNames.AddRange(originBundleAssets[item.Key]);
                packedAssets.AddRange(item.Value);
                buildData.manifestWriteType = rule.manifestWriteType;
                AddBuildData(buildData, rule);
            }
            return true;
        }

        public static bool GenRuleBuildByAssetBundleName(BuildRule rule)
        {
            UnityEditor.EditorUtility.DisplayProgressBar(string.Format("Packing index At({0}/{1})...", CurRuleIndex, rules.Count), rule.searchPath, CurRuleIndex * 1f / rules.Count);
            var files = GetFilesWithoutPacked(rule.searchPath, rule.searchPattern, rule.searchOption);
            if (files.Count == 0)
            {
                return true;
            }
            List<string> assetNames = new List<string>();
            if (rule.forceInclueDeps)
            {
                foreach (var item in files)
                {
                    assetNames.AddRange(GetAssetDependencies(item));
                }
            }
            assetNames.AddRange(files);


            AssetBundleBuildData buildData = new AssetBundleBuildData();
            buildData.originAssetNames.AddRange(files);
            if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
            {
                buildData.assetBundleName = BuildAssetBundleNameWithAssetPath(rule.overrideBundleName);
            }
            else
            {
                buildData.assetBundleName = BuildAssetBundleNameWithAssetPath(rule.searchPath);
            }
            if (!CheckAssetBundleName(buildData.assetBundleName))
            {
                return false;
            }
            buildData.assetNames.AddRange(assetNames);
            packedAssets.AddRange(assetNames);
            buildData.manifestWriteType = rule.manifestWriteType;
            AddBuildData(buildData, rule);
            return true;
        }

        public static List<string> GetAssetDependencies(string item)
        {
            var dependencies = AssetDatabase.GetDependencies(item);
            List<string> depsList = new List<string>();
            foreach (var assetPath in dependencies)
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }
                if (assetPath == item)
                {
                    continue;
                }
                //已经设置单独打包了，不需要再打包了
                if (packedAssets.Contains(assetPath))
                {
                    continue;
                }
                bool isNeedPackFile = true;
                //不打包.cs文件等类型的资源
                if (Consts.NoPackedDependenciesFiles.Contains(Path.GetExtension(assetPath).ToLower()))
                {
                    isNeedPackFile = false;
                }
                if (onlyBuildCurLang && buildLangRegex.IsMatch(assetPath))
                {
                    isNeedPackFile = false;
                }
                if (!isNeedPackFile)
                {
                    continue;
                }
                depsList.Add(assetPath);
            }
            return depsList;
        }

        public static void CollectDependencies()
        {
            int count = 0;
            foreach (var item in packedAssets)
            {
                count++;
                UnityEditor.EditorUtility.DisplayProgressBar(string.Format("Collecting Dependencies... [{0}/{1}]", count, packedAssets.Count), item, count * 1f / packedAssets.Count);
                var dependencies = GetAssetDependencies(item);
                foreach (var assetPath in dependencies)
                {
                    if (!allDependencies.ContainsKey(assetPath))
                    {
                        allDependencies[assetPath] = new HashSet<string>();
                    }
                    if (!allDependencies[assetPath].Contains(item))
                    {
                        allDependencies[assetPath].Add(item);
                    }
                }
            }
        }

        //打包依赖资源
        public static bool GenDependenciesBuild()
        {
            List<string> sharedAssetList = new List<string>();
            foreach (var item in allDependencies)
            {
                //去除不要文件
                if (Consts.NoPackDependFiles.Contains(Path.GetExtension(item.Key).ToLower()))
                {
                    continue;
                }

                //未达到成为公共资源的依赖数量
                if (item.Value.Count < Consts.CommonAssetRelyCount)
                {
                    continue;
                }

                sharedAssetList.Add(item.Key);
            }


            //开始打包共享依赖资源

            Dictionary<string, AssetBundleBuildData> sharedBuildDatas = new Dictionary<string, AssetBundleBuildData>();
            Regex[] shareRegexList = new Regex[shareRules.Count];

            //生成正则匹配对象
            for (int k = 0; k < shareRules.Count; k++)
            {
                Regex rex = new Regex(shareRules[k].searchPattern, RegexOptions.IgnoreCase);
                shareRegexList[k] = rex;
            }

            foreach (var shareAssetName in sharedAssetList)
            {
                bool isInBuild = false;
                for (int k = 0; k < shareRules.Count; k++)
                {
                    var rule = shareRules[k];
                    if (shareAssetName.StartsWith(rule.searchPath))
                    {
                        if (shareRegexList[k].IsMatch(shareAssetName))
                        {
                            string assetBundleName = string.Empty;
                            switch (rule.buildType)
                            {
                                case RuleBuilType.DirectoryName:
                                    assetBundleName = BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(shareAssetName), true);
                                    break;
                                case RuleBuilType.AssetBundleName:
                                    if (rule.isOverrideBundleName && !string.IsNullOrEmpty(rule.overrideBundleName))
                                    {
                                        assetBundleName = BuildAssetBundleNameWithAssetPath(rule.overrideBundleName, true);
                                    }
                                    else
                                    {
                                        assetBundleName = BuildAssetBundleNameWithAssetPath(rule.searchPath, true);
                                    }
                                    break;
                            }
                            if (!CheckAssetBundleName(assetBundleName))
                            {
                                return false;
                            }
                            if (!sharedBuildDatas.TryGetValue(assetBundleName, out var sharebuildData))
                            {
                                sharebuildData = new AssetBundleBuildData();
                                sharebuildData.isCommonAssetBundle = true;
                                sharebuildData.assetBundleName = assetBundleName;
                                AddBuildData(sharebuildData, rule);
                                sharedBuildDatas.Add(assetBundleName, sharebuildData);
                            }
                            sharebuildData.assetNames.Add(shareAssetName);
                            isInBuild = true;
                            break;
                        }
                    }
                }

                //如果没进共享资源包规则，就自成一包
                if (!isInBuild)
                {
                    AssetBundleBuildData sharebuildData = new AssetBundleBuildData();
                    sharebuildData.isCommonAssetBundle = true;
                    sharebuildData.assetBundleName = BuildAssetBundleNameWithAssetPath(shareAssetName, true);
                    if (!CheckAssetBundleName(sharebuildData.assetBundleName))
                    {
                        return false;
                    }
                    sharebuildData.assetNames.Add(shareAssetName);
                    AddBuildData(sharebuildData);
                }
            }

            return true;
        }

        public static bool GenBuildList(AssetBundleBuildData buildData)
        {
            if (!buildData.isCommonAssetBundle)
            {
                HashSet<string> withoutShareAssets = new HashSet<string>();
                foreach (var item in buildData.assetNames)
                {
                    withoutShareAssets.AddRange(GetAssetDependenciesWithoutShare(item));
                }
                int sceneCount = GetSceneAssetCount(buildData.assetNames);
                if (sceneCount > 0)
                {
                    if (sceneCount != buildData.assetNames.Count)
                    {
                        Debug.LogError("AssetBundle Error:场景的AssetBundle不能包含其他资源!");
                        return false;
                    }
                }
                else
                    buildData.assetNames.AddRange(withoutShareAssets);
            }
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = buildData.assetBundleName;
            build.assetNames = buildData.assetNames.ToArray();
            builds.Add(build);
            return true;
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

        public static List<string> GetAssetDependenciesWithoutShare(string item)
        {
            var files = GetAssetDependencies(item);
            var removeAll = files.RemoveAll((string assetPath) =>
            {
                return Consts.NoPackDependFiles.Contains(Path.GetExtension(assetPath).ToLower()) || (allDependencies.ContainsKey(assetPath) && allDependencies[assetPath].Count >= Consts.CommonAssetRelyCount);
            });
            return files;
        }


        public static List<string> GetFilesWithoutPacked(string searchPath, string searchPattern, SearchOption searchOption)
        {
            var files = GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
            var removeAll = files.RemoveAll((string obj) =>
            {
                bool needRemove = false;
                do
                {
                    if (Consts.NoPackedFiles.Contains(Path.GetExtension(obj).ToLower()))
                    {
                        needRemove = true;
                        break;
                    }
                    if(onlyBuildCurLang && buildLangRegex.IsMatch(obj))
                    {
                        needRemove = true;
                        break;
                    }
                    string path = obj.Replace("\\", "/");
                    if (trieTreeFilter.GetPathValueType(path) == PathTrieTree.PathValueType.BlackList)
                    {
                        needRemove = true;
                        break;
                    }
                    if (!needRemove)
                    {
                        foreach (var noPackDir in Consts.NoPackedDirs)
                        {
                            if (path.Contains(noPackDir))
                            {
                                needRemove = true;
                                break;
                            }
                        }
                    }

                } while (false);
               
                if (needRemove)
                {
                    return true;
                }
                return packedAssets.Contains(obj);
            });
            return files;
        }

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

        public static void AddRange<T>(this HashSet<T> setMap, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!setMap.Contains(list[i]))
                    setMap.Add(list[i]);
            }
        }

        public static void AddRange<T>(this HashSet<T> setMap, T[] list)
        {
            for (int i = 0; i < list.Length; i++)
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
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.Utilities;

namespace UniFan.ResEditor
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
        private static HashSet<string> packedAssets = new HashSet<string>();
        public static Dictionary<string, HashSet<string>> allDependencies = new Dictionary<string, HashSet<string>>();
        public static List<AssetBundleBuildData> buildDatas = new List<AssetBundleBuildData>();
        public static HashSet<string> depCullingBundles = new HashSet<string>();        //需要剔除依赖的
        public static HashSet<string> depCullingIgnoreBundles = new HashSet<string>();  //忽略剔除规则的包
        //正则匹配过的数据，用来避免重复匹配相同文件
        public static Dictionary<string, bool> regexMatched = new Dictionary<string, bool>();
        //查找过的依赖数据，避免重复查找相同文件的依赖
        public static Dictionary<string, string[]> dependenciesCache = new Dictionary<string, string[]>();
        //相同包里面的文件放在一起，避免多个同名包
        public static Dictionary<string, AssetBundleBuildData> newBuildDatas = new Dictionary<string, AssetBundleBuildData>();

        //筛选字典树
        public static PathTrieTree trieTreeFilter;

        public static int CurRuleIndex;

        /// <summary>
        /// 所有的打包规则类型
        /// </summary>
        private static IRulePacker[] rulePackers;
        static ABBuildCreator()
        {
            rulePackers = new IRulePacker[Enum.GetValues(typeof(RulePackerType)).Length];
            rulePackers[(int)RulePackerType.FileName] = new RulePackerByFileName();
            rulePackers[(int)RulePackerType.DirectoryName] = new RulePackerByDirectoryName();
            rulePackers[(int)RulePackerType.AssetBundleName] = new RulePackerByAssetBundleName();
            rulePackers[(int)RulePackerType.TobSubDirectoryName] = new RulePackerByTopSubDirName();
        }

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
            regexMatched.Clear();
            dependenciesCache.Clear();
            newBuildDatas.Clear();

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
            if (onlyBuildCurLang)
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
                if (rule.active && rule.buildType != RulePackerType.FileName)
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
            IRulePacker packer = rulePackers[(int)rule.buildType];
            if (packer == null)
            {
                Debug.LogError("Not found RulePackerType:" + rule.buildType);
                return false;
            }
            return packer.ResRulePacker(rule);
        }

        public static bool ShowRulePackerProgressBar(string packItem, int localIndex, int localTotalCount)
        {
            string title = string.Format("Packing index At({0}/{1})... progress:[{2}/{3}]", CurRuleIndex, rules.Count, localIndex, localTotalCount);
            return UnityEditor.EditorUtility.DisplayCancelableProgressBar(title, packItem, localIndex * 1f / localTotalCount);
        }

        public static bool ContainPackedAssets(string assetName)
        {
            return packedAssets.Contains(assetName);
        }

        public static void AddPackedAssets(ICollection<string> assetNames)
        {
            packedAssets.AddRange(assetNames);
        }

        public static List<string> GetAssetDependencies(string item)
        {
            if (!dependenciesCache.TryGetValue(item, out var dependencies))
            {
                dependencies = AssetDatabase.GetDependencies(item);
                dependenciesCache[item] = dependencies;
            }
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
                if (ContainPackedAssets(assetPath))
                {
                    continue;
                }
                bool isNeedPackFile = true;
                //不打包.cs文件等类型的资源
                if (Consts.NoPackedDependenciesFiles.Contains(Path.GetExtension(assetPath).ToLower()))
                {
                    isNeedPackFile = false;
                }

                if (!regexMatched.TryGetValue(assetPath, out var match))
                {
                    match = onlyBuildCurLang && buildLangRegex.IsMatch(assetPath);
                    regexMatched[assetPath] = match;
                }

                if (match)
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

                            IRulePacker packer = rulePackers[(int)rule.buildType];
                            if (packer == null)
                            {
                                Debug.LogError("Not found RulePackerType:" + rule.buildType);
                                return false;
                            }
                            string assetBundleName = packer.GetShareRulePackerName(rule, shareAssetName);

                            if (!ABBuildUtility.CheckAssetBundleName(assetBundleName))
                            {
                                return false;
                            }
                            if (!ABBuildUtility.CheckAssetBundleName(assetBundleName))
                            {
                                return false;
                            }
                            var sharebuildData = TryNewBuildData(assetBundleName);
                            sharebuildData.isCommonAssetBundle = true;
                            sharebuildData.assetNames.Add(shareAssetName);
                            isInBuild = true;
                            break;
                        }
                    }
                }

                //如果没进共享资源包规则，就自成一包
                if (!isInBuild)
                {
                    var assetBundleName = ABBuildUtility.BuildAssetBundleNameWithAssetPath(shareAssetName, true);
                    if (!ABBuildUtility.CheckAssetBundleName(assetBundleName))
                    {
                        return false;
                    }

                    var sharebuildData = TryNewBuildData(assetBundleName);
                    sharebuildData.isCommonAssetBundle = true;
                    sharebuildData.assetNames.Add(shareAssetName);
                }
            }

            return true;
        }

        public static AssetBundleBuildData TryNewBuildData(string assetBundleName, BuildRule rule = null)
        {
            var isNew = false;
            if (!newBuildDatas.TryGetValue(assetBundleName, out var buildData))
            {
                buildData = new AssetBundleBuildData
                {
                    assetBundleName = assetBundleName
                };
                buildDatas.Add(buildData);
                newBuildDatas.Add(assetBundleName, buildData);
                isNew = true;
            }

            if (rule != null)
            {
                if (((int)cullingLangType & rule.depCulling) > 0)
                {
                    //需要剔除引用的
                    depCullingBundles.Add(buildData.assetBundleName);
                    if (!isNew)
                    {
                        Debug.LogError($"depCullingBundles {buildData.assetBundleName}");
                    }
                }
                if (rule.ignoreDepCulling)
                {
                    //忽略引用剔除规则的包
                    depCullingIgnoreBundles.Add(buildData.assetBundleName);
                    if (!isNew)
                    {
                        Debug.LogError($"depCullingIgnoreBundles {buildData.assetBundleName}");
                    }
                }
            }
            return buildData;
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
                int sceneCount = ABBuildUtility.GetSceneAssetCount(buildData.assetNames);
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


        public static List<string> GetAssetDependenciesWithoutShare(string item)
        {
            var files = ABBuildCreator.GetAssetDependencies(item);
            var removeAll = files.RemoveAll((string assetPath) =>
            {
                return Consts.NoPackDependFiles.Contains(Path.GetExtension(assetPath).ToLower()) || (allDependencies.ContainsKey(assetPath) && allDependencies[assetPath].Count >= Consts.CommonAssetRelyCount);
            });
            return files;
        }

        public static List<string> GetFilesWithoutPacked(string searchPath, string searchPattern, SearchOption searchOption)
        {
            var files = ABBuildUtility.GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
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
                    if (!regexMatched.TryGetValue(obj, out var match))
                    {
                        match = onlyBuildCurLang && buildLangRegex.IsMatch(obj);
                        regexMatched[obj] = match;
                    }
                    if (match)
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
                return ContainPackedAssets(obj);
            });
            return files;
        }

    }
}

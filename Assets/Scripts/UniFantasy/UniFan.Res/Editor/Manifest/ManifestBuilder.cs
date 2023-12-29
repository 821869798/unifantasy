using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace UniFan.ResEditor
{
    public static class ManifestBuilder
    {
        public static bool BuildManifest(IResBuildAdapter resBuildAdapter)
        {
            List<AssetBundleBuildData> buildDataList = ABBuildCreator.buildDatas;
            HashSet<string> depCulling = ABBuildCreator.depCullingBundles;
            HashSet<string> depCullingIgnore = ABBuildCreator.depCullingIgnoreBundles;

            Dictionary<string, int> allAssetNameMap = new Dictionary<string, int>();
            List<string> allAssetNames = new List<string>();
            Dictionary<string, int> allBundleNameMap = new Dictionary<string, int>();
            int assetCount = 0;

            for (int i = 0; i < buildDataList.Count; i++)
            {
                var depAbList = resBuildAdapter.GetDirectDependencies(buildDataList[i].assetBundleName);

                //根据引用剔除逻辑
                if (depCullingIgnore.Contains(buildDataList[i].assetBundleName))
                {
                    buildDataList[i].dependencies.AddRange(depAbList);
                }
                else
                {
                    foreach (var depAb in depAbList)
                    {
                        if (!depCulling.Contains(depAb))
                        {
                            buildDataList[i].dependencies.Add(depAb);
                        }
                    }
                }

                buildDataList[i].hashValue = resBuildAdapter.GetAssetBundleHash(buildDataList[i].assetBundleName).ToString();
                switch (buildDataList[i].manifestWriteType)
                {
                    case ManifestWriteType.WriteAll:
                    case ManifestWriteType.OnlyWritePatternFile:
                        HashSet<string> assetSet = buildDataList[i].manifestWriteType == ManifestWriteType.WriteAll ? buildDataList[i].assetNames : buildDataList[i].originAssetNames;
                        foreach (var item in assetSet)
                        {
                            string shortName = item.Substring(7);
                            buildDataList[i].shortAssetNames.Add(shortName);
                            if (!allAssetNameMap.ContainsKey(shortName))
                            {
                                allAssetNames.Add(shortName);
                                allAssetNameMap[shortName] = assetCount;
                                assetCount++;
                            }
                        }
                        break;
                    case ManifestWriteType.NoWriteContianFile:
                        break;
                }
                allBundleNameMap[buildDataList[i].assetBundleName] = i;
            }

            //输出打包报告
            if (!BuildReporterText(buildDataList))
            {
                return false;
            }

            //检测ab循环引用
            if (!CheckCircularDependencies(buildDataList, allBundleNameMap))
            {
                return false;
            }

            //build manifest数据，游戏加载依赖于改自定义manifest
            if (!BuildManifestBinary(buildDataList, allAssetNameMap, allAssetNames, allBundleNameMap))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 使用拓扑排序，计算是否有ab包循环引用
        /// </summary>
        /// <returns></returns>
        public static bool CheckCircularDependencies(List<AssetBundleBuildData> buildDataList, Dictionary<string, int> allBundleNameMap)
        {
            //被依赖的次数
            int[] bDeps = new int[buildDataList.Count];
            //当前没有被其他依赖的队列
            Queue<int> que = new Queue<int>();
            //没有完成的列表，里面有循环引用
            HashSet<int> wrong = new HashSet<int>();
            for (int i = 0; i < buildDataList.Count; i++)
            {
                var buildData = buildDataList[i];
                foreach (var dep in buildData.dependencies)
                {
                    var index = allBundleNameMap[dep];
                    bDeps[index]++;
                }
                wrong.Add(i);
            }
            for (int i = 0; i < bDeps.Length; i++)
            {
                if (bDeps[i] == 0)
                {
                    que.Enqueue(i);
                }
            }
            while (que.Count > 0)
            {
                int i = que.Dequeue();
                wrong.Remove(i);
                var buildData = buildDataList[i];
                foreach (var dep in buildData.dependencies)
                {
                    var index = allBundleNameMap[dep];
                    bDeps[index]--;
                    if (bDeps[index] == 0)
                    {
                        que.Enqueue(index);
                    }
                }
            }
            if (wrong.Count > 0)
            {
                //打印冲突结果
                Debug.LogError(wrong.Count + "个AB包存在循环引用(error):");
                foreach (var i in wrong)
                {
                    var buildData = buildDataList[i];
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(buildData.assetBundleName);
                    sb.AppendLine("Assets:");
                    foreach (var asset in buildData.assetNames)
                    {
                        sb.AppendLine(asset);
                    }
                    sb.AppendLine("dependencies:");
                    foreach (var dep in buildData.dependencies)
                    {
                        sb.AppendLine(dep);
                    }
                    Debug.LogError(sb.ToString());
                }
                return false;
            }

            return true;
        }

        public static bool BuildReporterText(List<AssetBundleBuildData> buildDataList)
        {
            try
            {
                string outputPath = Path.Combine(ABBuildConsts.AssetBundlesOutputPath, ABBuildConsts.GetPlatformName());
                string filePath = Path.Combine(outputPath, ABBuildConsts.ResBuildReporterName);
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var item in buildDataList)
                        {
                            sw.WriteLine(item.assetBundleName);
                            sw.WriteLine(item.hashValue);
                            sw.WriteLine(item.shortAssetNames.Count);
                            foreach (var asset in item.shortAssetNames)
                            {
                                sw.WriteLine(asset);
                            }
                            sw.WriteLine(item.dependencies.Count);
                            foreach (var dep in item.dependencies)
                            {
                                sw.WriteLine(dep);
                            }
                            sw.WriteLine("");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError("Build Reporter Text Failed:" + e.ToString());
                return false;
            }
            AssetDatabase.Refresh();

            return true;
        }

        public static bool BuildManifestBinary(List<AssetBundleBuildData> buildDataList, Dictionary<string, int> allAssetNameMap, List<string> allAssetNames, Dictionary<string, int> allBundleNameMap)
        {
            try
            {
                string filePath = Path.Combine(Application.dataPath, ABBuildConsts.ResManifestBinaryConfigName);
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //写入头,ABD为AssetBundleData 
                        bw.Write(new char[] { 'A', 'B', 'D' });
                        //写入数据格式版本
                        bw.Write((int)1);
                        bw.Write(allAssetNames.Count);
                        for (int i = 0; i < allAssetNames.Count; i++)
                        {
                            bw.Write(allAssetNames[i]);
                        }
                        bw.Write((int)buildDataList.Count);
                        for (int i = 0; i < buildDataList.Count; i++)
                        {
                            AssetBundleBuildData buildData = buildDataList[i];
                            bw.Write(buildData.assetBundleName);
                            bw.Write(buildData.shortAssetNames.Count);
                            for (int j = 0; j < buildData.shortAssetNames.Count; j++)
                            {
                                string assetName = buildData.shortAssetNames[j];
                                bw.Write(allAssetNameMap[assetName]);
                            }
                            bw.Write(buildData.dependencies.Count);
                            for (int j = 0; j < buildData.dependencies.Count; j++)
                            {
                                string depName = buildData.dependencies[j];
                                bw.Write(allBundleNameMap[depName]);
                            }
                        }
                    }
                }
                AssetDatabase.Refresh();
                //打包储存AB包信息的文件
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = ABBuildConsts.ResManifestFilePath;
                build.assetNames = new string[] { string.Concat("Assets/", ABBuildConsts.ResManifestBinaryConfigName) };
                string outputPath = Path.Combine(ABBuildConsts.AssetBundlesOutputPath, ABBuildConsts.GetPlatformName());
                var options = BuildAssetBundleOptions.None;

                options |= BuildAssetBundleOptions.ChunkBasedCompression;

                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

                BuildPipeline.BuildAssetBundles(outputPath, new AssetBundleBuild[] { build }, options, EditorUserBuildSettings.activeBuildTarget);

                //删除多余文件
                string manifestAsset = Path.Combine(outputPath, ABBuildConsts.GetPlatformName());
                string manifestText = Path.Combine(outputPath, ABBuildConsts.GetPlatformName() + ".manifest");
                if (File.Exists(manifestAsset))
                {
                    FileUtil.DeleteFileOrDirectory(manifestAsset);
                }
                if (File.Exists(manifestText))
                {
                    FileUtil.DeleteFileOrDirectory(manifestText);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Build ManifestBinary Failed:" + e.ToString());
                return false;
            }


            return true;
        }
    }
}


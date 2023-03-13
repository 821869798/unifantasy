using System.Collections.Generic;
using System.IO;

namespace UniFan.Res
{
    public class ResManifest
    {
        public struct ResBundleData
        {
            //所有的资源
            public int[] assets;
            //直接引用,不包括间接的引用
            public int[] deps;
        }

        //Key为asset,Value为他的bundle的索引
        private readonly Dictionary<string, int> a2bMap = new Dictionary<string, int>();
        //Key为bundle,Value为bundle数据
        private readonly Dictionary<string, ResBundleData> bMap = new Dictionary<string, ResBundleData>();

        public string[] allAssets { get; private set; }

        public string[] allBundles { get; private set; }

        public int version { private set; get; }

        public string GetBundleName(string assetPath)
        {
            return allBundles[a2bMap[assetPath]];
        }

        public bool ContainsBundle(string bundle)
        {
            return bMap.ContainsKey(bundle);
        }

        public bool ContainsAsset(string assetPath)
        {
            return a2bMap.ContainsKey(assetPath);
        }

        public void GetBundleDependences(string bundleName, List<string> depsList)
        {
            ResBundleData bundleData;
            if (bMap.TryGetValue(bundleName, out bundleData))
            {
                for (int i = 0; i < bundleData.deps.Length; i++)
                {
                    int index = bundleData.deps[i];
                    depsList.Add(allBundles[index]);
                }
            }
        }


        public string[] GetBundleAssets(string bundleName)
        {
            return System.Array.ConvertAll<int, string>(bMap[bundleName].assets, input =>
            {
                return allAssets[input];
            });
        }

        void ResetData()
        {
            a2bMap.Clear();
            bMap.Clear();

            allAssets = null;
            allBundles = null;
            version = 0;
        }

        public void Load(BinaryReader br)
        {
            ResetData();

            char[] fileHeadChars = br.ReadChars(3);
            if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D')
                return;
            version = br.ReadInt32();

            int allAssetCount = br.ReadInt32();
            allAssets = new string[allAssetCount];
            for (int i = 0; i < allAssetCount; i++)
            {
                allAssets[i] = br.ReadString();
            }
            int bundleCount = br.ReadInt32();
            allBundles = new string[bundleCount];
            for (int i = 0; i < bundleCount; i++)
            {
                allBundles[i] = br.ReadString();
                ResBundleData bundleData = new ResBundleData();
                int assetCount = br.ReadInt32();
                bundleData.assets = new int[assetCount];
                for (int j = 0; j < assetCount; j++)
                {
                    int assetIndex = br.ReadInt32();
                    bundleData.assets[j] = assetIndex;
                    string assetName = allAssets[assetIndex];
                    if (!a2bMap.ContainsKey(assetName))
                    {
                        a2bMap.Add(assetName, i);
                    }
                }
                int depCount = br.ReadInt32();
                bundleData.deps = new int[depCount];
                for (int j = 0; j < depCount; j++)
                {
                    int depIndex = br.ReadInt32();
                    bundleData.deps[j] = depIndex;
                }
                bMap[allBundles[i]] = bundleData;
            }
        }
    }
}
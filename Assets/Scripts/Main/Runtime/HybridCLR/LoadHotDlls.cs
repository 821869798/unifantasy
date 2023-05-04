using Cysharp.Threading.Tasks;
using HybridCLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UniFan.Res;
using UnityEngine;

namespace Main
{
    public static class LoadHotDlls
    {

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        public static async UniTask LoadMetadataForAOTAssemblies(ResLoader resloader)
        {

            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 

            var fileListBinary = resloader.LoadABAsset<TextAsset>(HybridCLRUtil.CodeDllPath + HybridCLRUtil.AotFileListName);
            if (fileListBinary == null || fileListBinary.bytes == null)
            {
                Debug.LogError("LoadMetadataForAOTAssemblies FileList failed:" + HybridCLRUtil.AotFileListName);
                return;
            }
            List<string> aotFileList = new List<string>();
            try
            {
                using (MemoryStream ms = new MemoryStream(fileListBinary.bytes))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    var count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        aotFileList.Add(br.ReadString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Read AotFileList[{HybridCLRUtil.AotFileListName}] failed:{ex}");
            }


            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotFileList)
            {
                //byte[] dllBytes = BetterStreamingAssets.ReadAllBytes(aotDllName + ".bytes");
                var text = await resloader.LoadABAssetAwait<TextAsset>(HybridCLRUtil.CodeDllPath + aotDllName);
                if (text == null)
                {
                    return;
                }
                byte[] dllBytes = text.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }

        }

        /// <summary>
        /// 加载更新assembly
        /// </summary>
        /// <returns></returns>
        public static async UniTask<Assembly> LoadHotUpdateAssembly(ResLoader resloader, string fileName)
        {
            var filePath = HybridCLRUtil.CodeDllPath + fileName;

            var text = await resloader.LoadABAssetAwait<TextAsset>(filePath);
            if (text == null)
            {
                Debug.LogError($"Load {fileName} Failed");
                return null;
            }
            byte[] assemblyData = text.bytes;
            Assembly ass = Assembly.Load(assemblyData);

            return ass;
        }
    }
}

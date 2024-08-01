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
            List<string> aotFileList = new List<string>();

            try
            {
                var fileListBinary = await resloader.LoadABAssetAwait<TextAsset>(HybridCLRUtil.CodeDllPath + HybridCLRUtil.AOTMetadataPath + "/" + HybridCLRUtil.AotFileListName);
                if (fileListBinary == null || fileListBinary.bytes == null)
                {
                    Debug.LogError($"[{nameof(LoadMetadataForAOTAssemblies)}] FileList failed:" + HybridCLRUtil.AotFileListName);
                    return;
                }
                var bs = fileListBinary.bytes;
                resloader.ReleaseAllRes();
                using (MemoryStream ms = new MemoryStream(bs))
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
                Debug.LogError($"[{nameof(LoadMetadataForAOTAssemblies)}] Read AotFileList[{HybridCLRUtil.AotFileListName}] failed:{ex}");
            }

            try
            {
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                foreach (var aotDllName in aotFileList)
                {
                    //byte[] dllBytes = BetterStreamingAssets.ReadAllBytes(aotDllName + ".bytes");
                    var text = await resloader.LoadABAssetAwait<TextAsset>(HybridCLRUtil.CodeDllPath + HybridCLRUtil.AOTMetadataPath + "/" + aotDllName);
                    if (text == null)
                    {
                        return;
                    }
                    byte[] dllBytes = text.bytes;
                    resloader.ReleaseAllRes();
                    // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                    LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                    Debug.Log($"[{nameof(LoadMetadataForAOTAssemblies)}]:{aotDllName}. mode:{mode} ret:{err}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(LoadMetadataForAOTAssemblies)}] exception:{e}");
            }


        }

        /// <summary>
        /// 加载更新assembly
        /// </summary>
        /// <returns></returns>
        public static async UniTask<Assembly> LoadHotUpdateAssembly(ResLoader resloader, string fileName)
        {
            try
            {
                var filePath = HybridCLRUtil.CodeDllPath + HybridCLRUtil.HotDllPath + "/" + fileName;
                var text = await resloader.LoadABAssetAwait<TextAsset>(filePath);
                if (text == null)
                {
                    Debug.LogError($"[{nameof(LoadHotUpdateAssembly)}] Load {fileName} Failed");
                    return null;
                }
                byte[] assemblyData = text.bytes;
                resloader.ReleaseAllRes();
                Assembly ass = Assembly.Load(assemblyData);

                return ass;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(LoadHotUpdateAssembly)}] exception:{e}");
                return null;
            }
        }

        public static async UniTask<Dictionary<string, Assembly>> LoadAllHotUpdateAssembly(ResLoader resloader, Func<string, bool> validLoadFunc = null)
        {
            List<string> hotDllFileList = new List<string>();

            Dictionary<string, Assembly> result = new Dictionary<string, Assembly>();
            try
            {
                var fileListBinary = await resloader.LoadABAssetAwait<TextAsset>(HybridCLRUtil.CodeDllPath + HybridCLRUtil.HotDllPath + "/" + HybridCLRUtil.HotDllFileListName);

                if (fileListBinary == null || fileListBinary.bytes == null)
                {
                    Debug.LogError($"[{nameof(LoadAllHotUpdateAssembly)}] FileList failed:" + HybridCLRUtil.AotFileListName);
                    return result;
                }
                var bs = fileListBinary.bytes;
                resloader.ReleaseAllRes();
                using (MemoryStream ms = new MemoryStream(bs))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    var count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var name = br.ReadString();
                        if (validLoadFunc == null || validLoadFunc(name))
                        {
                            hotDllFileList.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(LoadAllHotUpdateAssembly)}] Read HotDllFileList[{HybridCLRUtil.HotDllFileListName}] failed:{ex}");
            }

            // load
            foreach (var hotDllName in hotDllFileList)
            {
                var assembly = await LoadHotUpdateAssembly(resloader, hotDllName);
                if (assembly != null)
                {
                    result[assembly.GetName().Name] = assembly;
                }
            }
            return result;
        }

    }
}

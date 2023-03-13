using HybridCLR;
using System.Collections;
using System.Collections.Generic;
using UniFan.Res;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.Threading.Tasks;

public static class LoadHotDll
{

    public static string DllPath = "Res/Codes/";

    public static async Task StartLoadHotDllAsync()
    {
        ResLoader resloader = ResLoader.Create();
        await LoadMetadataForAOTAssemblies(resloader);
        await LoadHotUpdateAssembly(resloader);
        resloader.Put2Pool();
    }


    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    static async Task LoadMetadataForAOTAssemblies(ResLoader resloader)
    {
        //AOT泛型添加处
        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll.bytes",
            "System.dll.bytes",
            "System.Core.dll.bytes",
        };
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            //byte[] dllBytes = BetterStreamingAssets.ReadAllBytes(aotDllName + ".bytes");
            var handle = resloader.LoadABAssetAsyncAwait(DllPath + aotDllName);
            await handle;
            var text = handle.Result as TextAsset;
            if(text == null)
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
    static async Task LoadHotUpdateAssembly(ResLoader resloader)
    {
        var filePath = DllPath + "Assembly-CSharp.dll.bytes";

        if (!ResManager.Instance.ContainsAsset(filePath))
        {
            Debug.LogError($"not find:{filePath},please use MenuItem : HybridCLR/BuildCodesAndCopy");
            return;
        }

        var handle = resloader.LoadABAssetAsyncAwait(filePath);
        await handle;
        var text = handle.Result as TextAsset;
        if (text == null)
        {
            return;
        }
        byte[] assemblyData = text.bytes;
        Assembly ass = Assembly.Load(assemblyData);

        var t = ass.GetType("HotMain");
        if (t != null)
        {
            t.InvokeMember("EnterHotMain", BindingFlags.InvokeMethod, null, null, null);
        }
    }

}

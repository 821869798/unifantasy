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
    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    static async Task LoadMetadataForAOTAssemblies(ResLoader resloader)
    {
        //AOT������Ӵ�
        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll.bytes",
            "System.dll.bytes",
            "System.Core.dll.bytes",
        };
        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
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
            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }

    }

    /// <summary>
    /// ���ظ���assembly
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

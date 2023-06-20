using System.Collections.Generic;

namespace UniFan.Res
{
    public enum ResState
    {
        Waiting = 0,
        Loading = 1,
        Ready = 2,
    }

    public enum ResType
    {
        Resource = 0,
        AssetBundle = 1,
        ABAsset = 2,
    }

    public interface IRes : IRefCounter, IEnumeratorTask, IReusableClass, IAsyncTask
    {
        string AssetName { get; }

        ResState State { get; }

        ResType ResType { get; }

        UnityEngine.Object Asset { get; }

        float Progress { get; }

        bool Load();

        void LoadAsync();

        List<IRes> GetAndRetainDependResList();

        bool ReleaseRes();

        void Put2Pool();
    }
}

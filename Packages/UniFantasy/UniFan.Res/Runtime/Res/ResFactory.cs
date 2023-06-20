using System;
using System.Collections.Generic;

namespace UniFan.Res
{
    internal static class ResFactory
    {
        internal static IRes Create(string assetName, ResType resType)
        {
            switch (resType)
            {
                case ResType.ABAsset:
                    return ABAssetRes.Create(assetName);
                case ResType.AssetBundle:
                    return AssetBundleRes.Create(assetName);
                case ResType.Resource:
                    return ResourcesRes.Create(assetName);
            }
            return null;
        }
    }


}

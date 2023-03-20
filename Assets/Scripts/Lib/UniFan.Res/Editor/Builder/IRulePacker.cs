using System;
using System.Collections.Generic;

namespace UniFan.Res.Editor
{
    interface IRulePacker
    {
        bool ResRulePacker(BuildRule rule);

        string GetShareRulePackerName(BuildRule rule, string shareAssetName);
    }
}

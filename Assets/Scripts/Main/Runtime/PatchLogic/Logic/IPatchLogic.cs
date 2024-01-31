using Cysharp.Threading.Tasks;
using System;

namespace Main.HotUpdate
{
    public interface IPatchLogic : IDisposable
    {
        UniTask Run(PatchLogicContext patchContext);
    }
}

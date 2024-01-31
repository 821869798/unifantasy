using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Main.HotUpdate
{
    internal class PatchLogic_FinishPatchDone : IPatchLogic
    {
        public UniTask Run(PatchLogicContext patchContext)
        {
            Debug.LogWarning("热更完成!");
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {

        }


    }
}

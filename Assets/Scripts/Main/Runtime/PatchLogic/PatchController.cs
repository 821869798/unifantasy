using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Main.HotUpdate
{
    /// <summary>
    /// 热更补丁控制器
    /// </summary>
    public class PatchController : IDisposable
    {
        /// <summary>
        /// 实现一个更轻量的类fsm逻辑
        /// </summary>
        private readonly Dictionary<Type, IPatchLogic> patchLogics = new Dictionary<Type, IPatchLogic>();

        /// <summary>
        /// 上下文数据，用于各个逻辑之间传递数据
        /// </summary>
        public PatchLogicContext patchLogicContext { private set; get; }

        public PatchController Init()
        {

            patchLogicContext = new PatchLogicContext(this);

            // 一般是从上至下的顺序执行,这句话只是提示看代码的顺序
            AddPatchLogic<PatchLogic_RequstVersionInfo>();
            AddPatchLogic<PatchLogic_LoadLocalCacheVersionData>();
            AddPatchLogic<PatchLogic_RequestPatchManifest>();
            AddPatchLogic<PatchLogic_VerifyExistResourceFiles>();
            AddPatchLogic<PatchLogic_CreatePatchDownloader>();
            AddPatchLogic<PatchLogic_DownloadPatchFiles>();
            AddPatchLogic<PatchLogic_AfterDownloadComplete>();
            AddPatchLogic<PatchLogic_FinishPatchDone>();

            return this;
        }

        private void AddPatchLogic<T>() where T : IPatchLogic, new()
        {
            var logic = new T();
            patchLogics.Add(typeof(T), logic);
        }

        private void GetPatchLogic<T>(out T logic) where T : IPatchLogic
        {
            if (patchLogics.TryGetValue(typeof(T), out var logicObj))
            {
                logic = (T)logicObj;
            }
            else
            {
                logic = default;
            }
        }

        public UniTask RunPatchLogic<T>() where T : IPatchLogic
        {
            if (patchLogics.TryGetValue(typeof(T), out var logic))
            {
                return logic.Run(patchLogicContext);
            }
            return UniTask.CompletedTask;
        }

        public UniTask StartPatch()
        {
            return RunPatchLogic<PatchLogic_RequstVersionInfo>();
        }

        public void Dispose()
        {
            if (patchLogicContext != null)
            {
                patchLogicContext.Dispose();
                patchLogicContext = null;
            }
            foreach (var kv in patchLogics)
            {
                kv.Value.Dispose();
            }
            patchLogics.Clear();
        }


    }
}

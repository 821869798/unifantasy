using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.HotUpdate
{
    /// <summary>
    /// 请求远程版本信息，对比app版本
    /// </summary>
    internal class PatchLogic_RequstVersionInfo : IPatchLogic
    {

        public async UniTask Run(PatchLogicContext patchContext)
        {
            List<PatchRemoteVersion> remoteVersionList = null;
            while (true)
            {
                remoteVersionList = await RequstRemoteVersionList();

                if (remoteVersionList == null || remoteVersionList.Count == 0)
                {
                    // TODO 错误反馈，弹框
                    Debug.LogError($"RequstRemoteVersionList failed");

                    // 加这句只是防止卡死，加了弹框之后可以去除。
                    await UniTask.Delay(3000);
                }
                else
                {
                    break;
                }
            }


            // patchContext.remoteVersion 为匹配到的最佳远程版本
            foreach (var r in remoteVersionList)
            {
                if (r.appVersion == patchContext.localAppVersion)
                {
                    // 如是是当前的本地版本，就是最佳
                    patchContext.remoteVersion = r;
                    break;
                }
                if (patchContext.remoteVersion == null || r.appVersion > patchContext.remoteVersion.appVersion)
                {
                    // 否则使用最新的版本
                    patchContext.remoteVersion = r;
                }
            }

            // 比较app整包版本
            if (patchContext.remoteVersion.appVersion > patchContext.localAppVersion)
            {
                // TODO 需要整包更新

                Debug.LogWarning($"<TODO>[{nameof(PatchLogic_LoadLocalCacheVersionData)}] whole package update");
                return;
            }
            else if (patchContext.remoteVersion.appVersion < patchContext.localAppVersion)
            {
                // 本地包的版本比服务器包的版本高，这种情况不应该出现
                Debug.LogError($"<ERROR>[{nameof(PatchLogic_LoadLocalCacheVersionData)}] local app version is higher than remote app version");
                return;
            }


            await patchContext.RunPatchLogic<PatchLogic_LoadLocalCacheVersionData>();
        }

        /// <summary>
        /// 通过WebRequest请求远程版本信息
        /// 为啥结果是List呢，主要是考虑可以兼容多app版本的情况，比如只是修了bug，提升了app版本，但是不需要强制更新整包，允许多个app版本存在，并且可以复用热更版本
        /// </summary>
        async UniTask<List<PatchRemoteVersion>> RequstRemoteVersionList()
        {
            // todo通过网络请求远程版本信息
            //try
            //{
            //    using (var request = UnityWebRequest.Post("xxx", "xx"))
            //    {
            //        while (true)
            //        {
            //            await request.SendWebRequest();
            //            if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
            //            {
            //                // todo 失败提示弹框,点击完就重试
            //                // 可以用AutoResetUniTaskCompletionSource自定义等待UI操作完毕，可参考UILogin写法
            //                // 这里热更界面UI是特殊UI，不是用UIManager来管理的
            //                // await UIXXXX.ShowRequestFailed();
            //                continue;
            //            }
            //            // 成功
            //            break;
            //        }
            //    }
            //}catch(UnityWebRequestException e)
            //{

            //}


            // 这句之后可以删除，现在只有排除async方法的警告
            await UniTask.Yield();

            // TODO，通过url请求远程版本信息
            // 目前没有就写死了假数据
            return new List<PatchRemoteVersion>() {
                new PatchRemoteVersion
                {
                    appVersion = new Version("1.0.0"),
                    resVersion = new Version("1.0.0.1"),
                }
            };
        }

        public void Dispose()
        {

        }
    }
}

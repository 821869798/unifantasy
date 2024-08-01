using UnityEngine;
using UniFan;
using System.Globalization;
using UniFan.Res;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.IO;
using Main.HotUpdate;
using System;

namespace Main
{
    public class GameLauncher : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            LaunchGame().Forget();
        }

        async UniTask LaunchGame()
        {
            // 框架初始化
            UniFantasy.InitUniFantasy(this.gameObject);

            // 全局设置
            GameGlobalSetting();

            //热更相关
            await HotUpdate();

            //初始化资源管理
            ResManager.Instance.InitAssetBundle();

            InitAOTManager();

            // 开始加载dll
            await StartLoadHotDllAsync();

            Destroy(this);
        }

        /// <summary>
        /// 一些全局设置
        /// </summary>
        private void GameGlobalSetting()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //环境保持一致，不受地区影响
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// 绑定aot程序集中的管理器
        /// </summary>
        void InitAOTManager()
        {
            ManagerCenter.Instance.BindManage(ResManager.Instance);
        }


        async UniTask HotUpdate()
        {
#if UNITY_EDITOR
            if (!PatchLogicUtility.ActiveEditorPatchLogic)
            {

            }
            else
#endif
            {
#if UNITY_EDITOR
                if (AssetBundleUtility.ActiveBundleMode)
                {
                    Debug.LogWarning($"[{nameof(GameLauncher)}] 开启了编辑器热更模式，但是没有开启编辑器ab模式");
                }
#endif

#if UNITY_EDITOR
                //TODO 做热更逻辑，目前仅编辑器有效，之后做完正式的，可以去除该编辑器宏
                using (var patchController = new PatchController().Init())
                {
                    await patchController.StartPatch();
                }
#endif
            }
        }

        async UniTask StartLoadHotDllAsync()
        {
            const string HotDllAssemblyName = "HotCode.Runtime";
            Assembly assembly;

#if UNITY_EDITOR
            if (!HybridCLRUtil.HybridCLREditActive)
            {
                //编辑器直接反射调用本地dll
                string dllPath = Path.Combine(Directory.GetCurrentDirectory(), "Library", "ScriptAssemblies", HotDllAssemblyName + ".dll");
                assembly = Assembly.LoadFile(dllPath);
            }
            else
#endif
            {
                ResLoader resloader = ResLoader.Create();
                await LoadHotDlls.LoadMetadataForAOTAssemblies(resloader);
                var assemblyMap = await LoadHotDlls.LoadAllHotUpdateAssembly(resloader);
                assemblyMap.TryGetValue(HotDllAssemblyName, out assembly);
                resloader.Put2Pool();
            }

            if (assembly == null)
            {
                Debug.LogError($"Load HotUpdate Assembly {HotDllAssemblyName} failed!");
                return;
            }
            var t = assembly.GetType("HotCode.Game.HotMain");
            if (t != null)
            {
                Debug.Log($"Load HotUpdate Assembly {HotDllAssemblyName} Success!");
                t.InvokeMember("EnterHotMain", BindingFlags.InvokeMethod, null, null, null);
            }
            else
            {
                Debug.LogError("Not find type HotMain");
            }

        }

    }

}

using UnityEngine;
using UniFan;
using System.Globalization;
using UniFan.Res;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Main
{
    public class GameLauncher : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            UniFantasy.InitUniFantasy(this.gameObject);

            GameGlobalSetting();

            //TODO 热更相关

            //初始化资源管理
            ResManager.Instance.InitAssetBundle();

            InitManager();

            // 开始加载dll
            StartLoadHotDllAsync().Forget();

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

        void InitManager()
        {
            ManagerCenter.Instance.BindManage(ResManager.Instance);
        }


        async UniTask StartLoadHotDllAsync()
        {
            Assembly assembly;
#if UNITY_EDITOR
            if (!HybridCLRUtil.HybridCLREditActive)
            {
                //编辑器直接反射调用本地dll
                string dllPath = Path.Combine(Directory.GetCurrentDirectory(), "Library", "ScriptAssemblies", "HotCode.Runtime.dll");
                assembly = Assembly.LoadFile(dllPath);
            }
            else
#endif
            {
                ResLoader resloader = ResLoader.Create();
                await LoadHotDlls.LoadMetadataForAOTAssemblies(resloader);
                assembly = await LoadHotDlls.LoadHotUpdateAssembly(resloader, "HotCode.Runtime.dll.bytes");
                resloader.Put2Pool();
            }

            if (assembly == null)
            {
                Debug.LogError("Load HotUpdate Assembly HotCode.Runtime.dll.bytes failed!");
                return;
            }
            var t = assembly.GetType("HotCode.Game.HotMain");
            if (t != null)
            {
                t.InvokeMember("EnterHotMain", BindingFlags.InvokeMethod, null, null, null);
                Debug.Log("LoadHotUpdateAssembly HotCode.Runtime.dll.bytes Success!");
            }
            else
            {
                Debug.LogError("Not find type HotMain");
            }

        }

    }

}

using UnityEngine;
using UniFan;
using System.Threading.Tasks;
using System.Globalization;
using UniFan.Res;

public class GameLauncher : MonoBehaviour
{
    // Start is called before the first frame update
    private async Task Start()
    {
        UniFantasy.InitUniFantasy(this.gameObject);

        GameGlobalSetting();

        //TODO 热更相关

        //初始化资源管理
        ResManager.Instance.InitAssetBundle();
        
        InitManager();

        // 开始加载dll
        await LoadHotDll.StartLoadHotDllAsync();

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


}

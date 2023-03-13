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

        //TODO �ȸ����

        //��ʼ����Դ����
        ResManager.Instance.InitAssetBundle();
        
        InitManager();

        // ��ʼ����dll
        await LoadHotDll.StartLoadHotDllAsync();

        Destroy(this);
    }

    /// <summary>
    /// һЩȫ������
    /// </summary>
    private void GameGlobalSetting()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //��������һ�£����ܵ���Ӱ��
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    void InitManager()
    {
        ManagerCenter.Instance.BindManage(ResManager.Instance);
    }


}

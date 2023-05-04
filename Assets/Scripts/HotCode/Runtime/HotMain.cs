using HotCode.Framework;
using HotCode.FrameworkPlay;
using UniFan;
using UnityEngine;

public class HotMain
{

    public static void EnterHotMain()
    {
        BindManagerInHot();

        LoadTestScene();
    }

    private static void BindManagerInHot()
    {
        ManagerCenter.Instance.BindManage(UIManager.Instance);
    }

    private static async void LoadTestScene()
    {
        await GSceneManager.Instance.LoadAbSceneAsync("Test01");
        await UIManager.Instance.ShowWindowAsync<UILogin>();
    }

}

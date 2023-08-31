using Cysharp.Threading.Tasks;
using HotCode.Framework;
using HotCode.FrameworkPlay;
using UniFan;

public class HotMain
{

    public static void EnterHotMain()
    {
        BindManagerInHot();

        LoadTestScene().Forget();
    }

    private static void BindManagerInHot()
    {
        ManagerCenter.Instance.BindManage(UIManager.Instance);
    }

    private static async UniTaskVoid LoadTestScene()
    {
        await GSceneManager.Instance.LoadAbSceneAsync("Test01");
        await UIManager.Instance.ShowWindowAsync<UILogin>();
    }

}

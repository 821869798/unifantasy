using Cysharp.Threading.Tasks;
using HotCode.Framework;
using UniFan;

namespace HotCode.Game
{
    public class HotMain
    {

        public static void EnterHotMain()
        {
            BindManagerInHot();

            LoadTestScene().Forget();
        }

        private static void BindManagerInHot()
        {
            ManagerCenter.Instance.BindManage(TimerManager.Instance);
            ManagerCenter.Instance.BindManage(UIManager.Instance);
            ManagerCenter.Instance.BindManage(GSceneManager.Instance);
        }

        private static async UniTaskVoid LoadTestScene()
        {
            await GSceneManager.Instance.LoadAbSceneAsync("Test01");
            await UIManager.Instance.ShowWindowAsync<UILogin>();
        }

    }

}


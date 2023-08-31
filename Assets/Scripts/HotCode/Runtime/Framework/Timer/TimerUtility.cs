
using Cysharp.Threading.Tasks;

namespace HotCode.Framework
{
    public static class TimerUtility
    {
        public static async UniTask AwaitTimer(this uint timerId)
        {
            await TimerManager.Instance.AwaitTimer(timerId);
        }

        public static void StopTimer(this uint timerId)
        {
            TimerManager.Instance.StopTimer(timerId);
        }
    }
}

# Unitask.DOTweenEx

原版Unitask中自带的DOTween有功能缺陷。

例如，在通过AwaitForComplete创建出等待Tween完成的Task的时候，如果没有完成的时候在某处把Tween给Kill掉了会导致Task对象不被释放(通过`Unitask Tracker`查看)

```csharp
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public class TestTween : MonoBehaviour
{
    CancellationTokenSource cts = new CancellationTokenSource();
    void Start()
    {
        Test().Forget();
    }

    async UniTask Test()
    {
        Test2().Forget();
        try
        {
            await this.transform.DOLocalMove(Vector3.zero, 5f).AwaitForComplete(cancellationToken: cts.Token);
            Debug.LogError("end");
        }
        catch (System.Exception e)
        {
            Debug.LogError("exception:" + e);
        }

    }

    async UniTask Test2()
    {
        await UniTask.Delay(2000);
        //cts.Cancel();
        this.transform.DOKill();
    }
}

```

修改代码后，可以让DOTween使用的UniTask的时候在Kill时候会触发Unitask的取消事件。

**（除了直接调用ToUnitask而不是AwaitForComplete的情况，因为这个ToUnitask就是Kill的时候完成任务）**

也就是可以通过try catch可以捕获到`System.OperationCanceledException` 取消异常。
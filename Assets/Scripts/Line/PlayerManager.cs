using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Default;
using UnityEngine;

namespace Line
{
    public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
    {
        [SerializeField] private float speed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            Instance.ProgressAsync().Forget();
        }

        private async UniTask ProgressAsync()
        {
            InputProvider.Instance.SubscribeActions();
            
            using var cts = new CancellationTokenSource();

            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate())
            {
                var mousePos = InputProvider.Instance.mousePosition;
                var mousePoint = LineGrid.Instance.GetMousePoint();
                UIPresenter.Instance.SetMousePointerPosition(mousePoint);

                var x = 0f;
                var y = 0f;
                var viewSize = LineGrid.Instance.viewSize;
                if (Mathf.Abs(mousePos.x - viewSize.x / 2) > viewSize.x / 2 * 0.8f)
                {
                    if (mousePos.x > viewSize.x / 2)
                    {
                        x = -speed;
                    }
                    else
                    {
                        x = speed;
                    }
                }

                if (Mathf.Abs(mousePos.y - viewSize.y / 2) > viewSize.y / 2 * 0.8f)
                {
                    if (mousePos.y > viewSize.y / 2)
                    {
                        y = -speed;
                    }
                    else
                    {
                        y = speed;
                    }
                }
                
                LineGrid.Instance.UpdateGrid(new Vector2(x,y));
            }
        }
    }
}
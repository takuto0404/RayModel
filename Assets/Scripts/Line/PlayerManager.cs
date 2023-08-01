using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Default;
using UnityEngine;

namespace Line
{
    public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
    {
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private LineInfo linePrefab;
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

            var task1 = SceneMoveAsync(cts.Token);
            var task2 = DrawLineAsync(cts.Token);
            var task3 = UIPresenter.Instance.UITaskAsync(cts.Token);
            await UniTask.WhenAny(task1, task2,task3);
        }

        private async UniTask SceneMoveAsync(CancellationToken ct)
        {
            while (true)
            {
                await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(ct))
                {
                    var mousePos = InputProvider.Instance.mousePosition;
                    var mousePoint = LineGrid.Instance.GetMousePoint();
                    UIPresenter.Instance.SetMousePointerPosition(mousePoint);

                    Move(mousePos);
                    LineManager.Instance.UpdateLinePosition();
                }

                return;
            }
        }

        private void Move(Vector2 mousePos)
        {
            var x = 0f;
            var y = 0f;
            var viewSize = LineGrid.Instance.viewSize;
            if (Mathf.Abs(mousePos.x - viewSize.x / 2) > viewSize.x / 2 * 0.95f)
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

            if (Mathf.Abs(mousePos.y - viewSize.y / 2) > viewSize.y / 2 * 0.95f)
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

        private async UniTask DrawLineAsync(CancellationToken ct)
        {
            while (true)
            {
                Debug.Log("while");
                await InputProvider.Instance.MouseClickAsync(ct);
                var startPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;
                var newLine = Instantiate(linePrefab, Vector2.zero, Quaternion.identity,canvasTransform);

                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);
                var drawLineTask = UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                    newLine.LineRenderer.SetPositions(new[] { startPos - LineGrid.Instance.totalMisalignment, LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f }),mergedCts.Token);
                var clickTask = InputProvider.Instance.MouseClickAsync(mergedCts.Token);
                await UniTask.WhenAny(drawLineTask, clickTask);
                newCts.Cancel();
                mergedCts.Cancel();
                
                var endPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f;
                
                newLine.Init(startPos,endPos + LineGrid.Instance.totalMisalignment,LineType.Mirror,new [] { MaterialType.Air });
                LineManager.Instance.CreateLineAsUI(newLine);
                if (ct.IsCancellationRequested) return;
            }
        }
    }
}
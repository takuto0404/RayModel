using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Default;
using RaySim;
using UnityEngine;

namespace Line
{
    public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
    {
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private LineInfo linePrefab;
        [SerializeField] private RayInfo rayPrefab;
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
            var task3 = UIPresenter.Instance.LineUITaskAsync(cts.Token);
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

        private async UniTask DrawRayAsync(CancellationToken ct)
        {
            while (true)
            {
                await InputProvider.Instance.LongPressAsync(ct);
                var startPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;
                var newRay = Instantiate(rayPrefab, Vector2.zero, Quaternion.identity,canvasTransform);

                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);
                var drawRayTask = UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                    newRay.LineRenderer.SetPositions(new[] { startPos - LineGrid.Instance.totalMisalignment, LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f }),mergedCts.Token);
                var longPressTask = InputProvider.Instance.LongPressAsync(mergedCts.Token);
                await UniTask.WhenAny(drawRayTask, longPressTask);
                newCts.Cancel();

                var endPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;

                if (startPos == endPos)
                {
                    Destroy(newRay.gameObject);
                    continue;
                }
                
                newRay.Init(startPos,endPos / startPos);
                RayManager.Instance.CreateRayAsUI(newRay);
                
                UIPresenter.Instance.MakeRayContents();
                if (ct.IsCancellationRequested) return;
            }
        }

        private async UniTask DrawLineAsync(CancellationToken ct)
        {
            while (true)
            {
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

                var endPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;

                if (startPos == endPos)
                {
                    Destroy(newLine.gameObject);
                    continue;
                }
                
                newLine.Init(startPos,endPos,LineType.Mirror,new [] { MaterialType.Air });
                LineManager.Instance.CreateLineAsUI(newLine);
                
                UIPresenter.Instance.MakeLineContents();
                if (ct.IsCancellationRequested) return;
            }
        }
    }
}
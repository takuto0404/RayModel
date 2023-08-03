using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Default;
using RaySim;
using UI;
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
            var task2 = DrawAsync(cts.Token);
            var task3 = UIPresenter.Instance.LineUITaskAsync(cts.Token);
            var task4 = UIPresenter.Instance.RayUITaskAsync(cts.Token);
            await UniTask.WhenAny(task1, task2,task3,task4);
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
                    RayManager.Instance.UpdateRayPosition();
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

        private async UniTask DrawAsync(CancellationToken ct)
        {
            while (true)
            {
                var newCts1 = new CancellationTokenSource();
                var mergedCts1 = CancellationTokenSource.CreateLinkedTokenSource(newCts1.Token, ct);
                var rayTask = InputProvider.Instance.LongPressAsync(mergedCts1.Token);
                var lineTask = InputProvider.Instance.MouseClickAsync(mergedCts1.Token);
                var result = await UniTask.WhenAny(rayTask, lineTask);
                newCts1.Cancel();

                GameObject prefab;
                if (result == 0)
                {
                    prefab = rayPrefab.gameObject;
                }
                else
                {
                    prefab = linePrefab.gameObject;
                }
                var newObject = Instantiate(prefab, Vector2.zero, Quaternion.identity,canvasTransform);
                var newLineOrRay = newObject.GetComponent<ILineBeAble>();
                
                var newCts2 = new CancellationTokenSource();
                var mergedCts2 = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts2.Token);
                
                var startPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;

                Debug.Log("タスク開始");
                var drawLineOrRayTask = UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                    newLineOrRay.GetUGUILineRenderer().SetPositions(new[] { startPos - LineGrid.Instance.totalMisalignment, LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f }),mergedCts2.Token);
                var clickTask = InputProvider.Instance.MouseClickAsync(mergedCts2.Token);
                
                await UniTask.WhenAny(drawLineOrRayTask, clickTask);
                Debug.Log("終わった...");
                newCts2.Cancel();
                var endPos = LineGrid.Instance.GetMousePoint() - LineGrid.Instance.viewSize / 2f + LineGrid.Instance.totalMisalignment;
                if (startPos == endPos)
                {
                    Destroy(newLineOrRay.GetGameObject());
                    continue;
                }
                
                if (result == 0)
                {
                    var ray = (RayInfo)newLineOrRay;
                    ray.Init(startPos,endPos - startPos);
                    RayManager.Instance.CreateRayAsUI(ray);
                
                    UIPresenter.Instance.MakeRayContents();
                    RayManager.Instance.UpdateRayPosition();
                }
                else
                {
                    var line = (LineInfo)newLineOrRay;
                    line.Init(startPos,endPos,LineType.Mirror,new [] { MaterialType.Air });
                    LineManager.Instance.CreateLineAsUI(line);
                
                    UIPresenter.Instance.MakeLineContents();
                    LineManager.Instance.UpdateLinePosition();
                }
                if (ct.IsCancellationRequested) return;
            }
        }
    }
}
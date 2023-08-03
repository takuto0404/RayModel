using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RaySim;
using UnityEngine;
using UnityEngine.UI;

namespace Line
{
    public class UIView : MonoBehaviour
    {
        [SerializeField] private Image point;
        
        [SerializeField] private LineInfoUI lineInfoUIPrefab;
        [SerializeField] private Transform lineScrollViewContent;
        [SerializeField] private Image lineMenuPanel;
        [SerializeField] private Button lineMenuButton;
        [SerializeField] private Button lineMenuBackButton;
        
        [SerializeField] private RayInfoUI rayInfoUIPrefab;
        [SerializeField] private Transform rayScrollViewContent;
        [SerializeField] private Image rayMenuPanel;
        [SerializeField] private Button rayMenuButton;
        [SerializeField] private Button rayMenuBackButton;

        private List<LineInfoUI> _createdLine = new ();
        private List<RayInfoUI> _createdRay = new ();
        private UniTaskCompletionSource _uts = new ();


        public async UniTask<LineInfoUI> LineButtonSelectAsync(CancellationToken ct)
        {
            while (true)
            {
                _uts = new UniTaskCompletionSource();
                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                if (_createdLine.Count == 0)
                    await UniTask.WaitWhile(() => _createdLine.Count == 0, cancellationToken: mergedCts.Token);
                
                var task1 = UniTask.WhenAny(_createdLine.Select(ui => ui.thisButton.OnClickAsync(mergedCts.Token)));
                var task2 = _uts.Task;
                
                var result = await UniTask.WhenAny(task1,task2);

                if (!result.hasResultLeft)
                {
                    newCts.Cancel();
                    continue;
                }
                
                return _createdLine[result.result];
            }
        }

        public void SetMousePointerPosition(Vector2 position)
        {
            point.rectTransform.position = position;
        }

        public async UniTask LineMenuButtonOnClickAsync(CancellationToken ct)
        {
            await lineMenuButton.OnClickAsync(ct);
        }

        public async UniTask LineMenuBackButtonOnClickAsync(CancellationToken ct)
        {
            await lineMenuBackButton.OnClickAsync(ct);
        }

        public async UniTask ShowLineMenu(CancellationToken ct)
        {
            lineMenuPanel.enabled = true;
            await lineMenuPanel.rectTransform.DOLocalMove(new Vector2(LineGrid.Instance.viewSize.x / 3, 0),0.5f).WithCancellation(ct);
        }

        public async UniTask HideLineMenu(CancellationToken ct)
        {
            await lineMenuPanel.rectTransform.DOLocalMove(new Vector2(LineGrid.Instance.viewSize.x * 2 / 3,0), 0.5f)
                .WithCancellation(ct);
            lineMenuPanel.enabled = false;
        }

        public LineInfoUI MakeLineContents(List<LineInfo> lineInfos,LineInfo selecting)
        {
            LineInfoUI selected = null;
            _createdLine.ForEach(ui => Destroy(ui.gameObject));
            _createdLine = new List<LineInfoUI>();
            for (var i = 0; i < lineInfos.Count; i++)
            {
                var lineInfo = lineInfos[i];
                var newUI = Instantiate(lineInfoUIPrefab, lineScrollViewContent);
                newUI.Init(lineInfo,i + 1);
                _createdLine.Add(newUI);
                if (lineInfo == selecting)
                {
                    newUI.SelectColor();
                    selected = newUI;
                }
            }
            _uts.TrySetResult();
            return selected;
        }
        
        
        public async UniTask<RayInfoUI> RayButtonSelectAsync(CancellationToken ct)
        {
            while (true)
            {
                _uts = new UniTaskCompletionSource();
                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                if (_createdRay.Count == 0)
                    await UniTask.WaitWhile(() => _createdRay.Count == 0, cancellationToken: mergedCts.Token);
                
                var task1 = UniTask.WhenAny(_createdRay.Select(ui => ui.thisButton.OnClickAsync(mergedCts.Token)));
                var task2 = _uts.Task;
                
                var result = await UniTask.WhenAny(task1,task2);

                if (!result.hasResultLeft)
                {
                    newCts.Cancel();
                    continue;
                }
                
                return _createdRay[result.result];
            }
        }
        public async UniTask RayMenuButtonOnClickAsync(CancellationToken ct)
        {
            await rayMenuButton.OnClickAsync(ct);
        }

        public async UniTask RayMenuBackButtonOnClickAsync(CancellationToken ct)
        {
            await rayMenuBackButton.OnClickAsync(ct);
        }

        public async UniTask ShowRayMenu(CancellationToken ct)
        {
            rayMenuPanel.enabled = true;
            await rayMenuPanel.rectTransform.DOLocalMove(new Vector2(-LineGrid.Instance.viewSize.x / 3, 0),0.5f).WithCancellation(ct);
        }

        public async UniTask HideRayMenu(CancellationToken ct)
        {
            await rayMenuPanel.rectTransform.DOLocalMove(new Vector2(-LineGrid.Instance.viewSize.x * 2 / 3,0), 0.5f)
                .WithCancellation(ct);
            rayMenuPanel.enabled = false;
        }

        public RayInfoUI MakeRayContents(List<RayInfo> rayInfos,RayInfo selecting)
        {
            RayInfoUI selected = null;
            _createdRay.ForEach(ui => Destroy(ui.gameObject));
            _createdRay = new List<RayInfoUI>();
            for (var i = 0; i < rayInfos.Count; i++)
            {
                var rayInfo = rayInfos[i];
                var newUI = Instantiate(rayInfoUIPrefab, rayScrollViewContent);
                newUI.Init(rayInfo,i + 1);
                _createdRay.Add(newUI);
                if (rayInfo == selecting)
                {
                    newUI.SelectColor();
                    selected = newUI;
                }
            }
            _uts.TrySetResult();
            return selected;
        }
    }
}
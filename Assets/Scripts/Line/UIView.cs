using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Line
{
    public class UIView : MonoBehaviour
    {
        [SerializeField] private Image point;
        [SerializeField] private LineInfoUI lineInfoUIPrefab;
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private Image lineMenuPanel;
        [SerializeField] private Button lineMenuButton;
        [SerializeField] private Button backButton;

        private List<LineInfoUI> _created = new ();
        private UniTaskCompletionSource _uts = new ();


        public async UniTask<LineInfoUI> ButtonSelectAsync(CancellationToken ct)
        {
            while (true)
            {
                _uts = new UniTaskCompletionSource();
                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                if (_created.Count == 0)
                    await UniTask.WaitWhile(() => _created.Count == 0, cancellationToken: mergedCts.Token);
                
                var task1 = UniTask.WhenAny(_created.Select(ui => ui.thisButton.OnClickAsync(mergedCts.Token)));
                var task2 = _uts.Task;
                
                var result = await UniTask.WhenAny(task1,task2);

                if (!result.hasResultLeft)
                {
                    newCts.Cancel();
                    continue;
                }
                
                return _created[result.result];
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

        public async UniTask BackButtonOnClickAsync(CancellationToken ct)
        {
            await backButton.OnClickAsync(ct);
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

        public LineInfoUI MakeContents(List<LineInfo> lineInfos,LineInfo selecting)
        {
            LineInfoUI selected = null;
            _created.ForEach(ui => Destroy(ui.gameObject));
            _created = new List<LineInfoUI>();
            for (var i = 0; i < lineInfos.Count; i++)
            {
                var lineInfo = lineInfos[i];
                var newUI = Instantiate(lineInfoUIPrefab, scrollViewContent);
                newUI.Init(lineInfo,i + 1);
                _created.Add(newUI);
                if (lineInfo == selecting)
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
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
            await lineMenuPanel.rectTransform.DOMove(new Vector2(LineGrid.Instance.viewSize.x / 3, 0),0.5f).WithCancellation(ct);
        }

        public async UniTask HideLineMenu(CancellationToken ct)
        {
            await lineMenuPanel.rectTransform.DOMove(new Vector2(LineGrid.Instance.viewSize.x * 2 / 3,0), 0.5f)
                .WithCancellation(ct);
            lineMenuPanel.enabled = false;
        }

        public void MakeContents(List<LineInfo> lineInfos)
        {
            for (var i = 0; i < lineInfos.Count; i++)
            {
                var lineInfo = lineInfos[i];
                var newUI = Instantiate(lineInfoUIPrefab, scrollViewContent);
                newUI.lineNumberText.text = (i + 1).ToString();
                newUI.lineTypeText.text = lineInfo.LineType.ToString();
                var interval = LineGrid.Instance.interval;
                newUI.startPointText.text = $"({lineInfo.StartPoint.x / interval},{lineInfo.StartPoint.y / interval})";
                newUI.endPointText.text = $"({lineInfo.EndPoint.x / interval},{lineInfo.EndPoint.y / interval})";
            }
        }
    }
}
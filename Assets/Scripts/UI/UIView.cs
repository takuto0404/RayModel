using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RaySim;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
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

        [SerializeField] private GameObject selectView;
        [SerializeField] private Button mirrorButton;
        [SerializeField] private Button boundaryButton;
        [SerializeField] private Button absorbButton;
        [SerializeField] private GameObject boundaryContent;
        [SerializeField] private TMP_InputField inText;
        [SerializeField] private TMP_InputField outText;
        [SerializeField] private Button goButton;

        [SerializeField] private GameObject rayColorView;
        [SerializeField] private TMP_InputField colorText;
        [SerializeField] private Button goRayButton;

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

        public async UniTask<int> RayColorSelectAsync(CancellationToken ct)
        {
            rayColorView.SetActive(true);
            colorText.text = "";
            await goRayButton.OnClickAsync(ct);
            rayColorView.SetActive(false);
            var wasCorrect = int.TryParse(colorText.text,out var num);
            if (!wasCorrect) num = 0;
            return num;
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

        public async UniTask<(LineType lineType, MaterialType[] materialTypes)> SelectLineType(CancellationToken ct)
        {
            boundaryContent.SetActive(false);
            inText.text = "";
            outText.text = "";
            selectView.SetActive(true);
            var selecting = LineType.Mirror;
            while (true)
            {
                if (selecting == LineType.Mirror)
                {
                    mirrorButton.GetComponent<Image>().color = new Color(0.8f, 0.5f, 0.5f);
                    boundaryButton.GetComponent<Image>().color = Color.white;
                    absorbButton.GetComponent<Image>().color = Color.white;
                }
                else if(selecting == LineType.Boundary)
                {
                    boundaryButton.GetComponent<Image>().color = new Color(0.8f,0.5f,0.5f);
                    mirrorButton.GetComponent<Image>().color = Color.white;
                    absorbButton.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    absorbButton.GetComponent<Image>().color = new Color(0.8f,0.5f,0.5f);
                    mirrorButton.GetComponent<Image>().color = Color.white;
                    boundaryButton.GetComponent<Image>().color = Color.white;
                }
                var task1 = goButton.OnClickAsync(ct);
                var task2 = mirrorButton.OnClickAsync(ct);
                var task3 = boundaryButton.OnClickAsync(ct);
                var task4 = absorbButton.OnClickAsync(ct);
                var result = await UniTask.WhenAny(task1, task2, task3,task4);
                if (result == 0)
                {
                    if (selecting == LineType.Mirror)
                    {
                        selectView.SetActive(false);
                        return (selecting, new [] { MaterialType.Air });
                    }
                    if(selecting == LineType.Boundary)
                    {
                        
                        var inType = GetMaterialType(inText.text.Trim());
                        var outType = GetMaterialType(outText.text.Trim());
                        selectView.SetActive(false);
                        return (selecting, new [] { inType, outType });
                    }
                    selectView.SetActive(false);
                    return (selecting, new[] { MaterialType.Air });
                }
                if (result == 1)
                {
                    boundaryContent.SetActive(false);
                    selecting = LineType.Mirror;
                }
                else if(result == 2)
                {
                    boundaryContent.SetActive(true);
                    selecting = LineType.Boundary;
                }
                else if(result == 3)
                {
                    boundaryContent.SetActive(false);
                    selecting = LineType.Absorb;
                }
            }
        }

        private MaterialType GetMaterialType(string text)
        {
            Enum.TryParse(typeof(MaterialType),text,out var materialType);
            return (MaterialType)materialType;
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
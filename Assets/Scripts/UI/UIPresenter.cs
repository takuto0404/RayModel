using System.Threading;
using Cysharp.Threading.Tasks;
using Default;
using Line;
using RaySim;
using UnityEngine;

namespace UI
{
    public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
    {
        [SerializeField] private UIView uiView;
        private LineInfoUI _beforeSelectLine = null;
        private LineInfoUI _selectingLine = null;
        private RayInfoUI _beforeSelectRay = null;
        private RayInfoUI _selectingRay = null;

        public void SetMousePointerPosition(Vector2 pos)
        {
            uiView.SetMousePointerPosition(pos);
        }
        public void MakeRayContents()
        {
            RayInfo rayInfo = null;
            if (_selectingRay != null)
            {
                rayInfo = _selectingRay.rayInfo;
            }
            _beforeSelectRay = uiView.MakeRayContents(RayManager.Instance.GetAllRays(),rayInfo);
        }
        private async UniTask SelectRayButtonAsync(CancellationToken ct)
        {
            while (true)
            {
                _beforeSelectRay = _selectingRay;
                _selectingRay = await uiView.RayButtonSelectAsync(ct);
                if (_beforeSelectRay != null)
                {
                    if (_beforeSelectRay == _selectingRay)
                    {
                        _selectingRay = null;
                        _beforeSelectRay.ResetColor();
                        continue;
                    }
                    _beforeSelectRay.ResetColor();
                }
                _selectingRay.SelectColor();
                if (ct.IsCancellationRequested) return;
                
            }
        }
        public async UniTask RayUITaskAsync(CancellationToken ct)
        {
            while (true)
            {
                await uiView.RayMenuButtonOnClickAsync(ct);
                if(_selectingRay != null)_selectingRay.ResetColor();
                await uiView.ShowRayMenu(ct);

                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                var task1 = SelectRayButtonAsync(mergedCts.Token);
                var task2 = uiView.RayMenuBackButtonOnClickAsync(mergedCts.Token);
                await UniTask.WhenAny(task1, task2);
                newCts.Cancel();

                await uiView.HideRayMenu(ct);
                if(_selectingRay != null)_selectingRay.ResetColor();

                if (ct.IsCancellationRequested) return;
            }
        }

        public void MakeLineContents()
        {
            LineInfo lineInfo = null;
            if (_selectingLine != null)
            {
                lineInfo = _selectingLine.lineInfo;
            }
            _beforeSelectLine = uiView.MakeLineContents(LineManager.Instance.GetAllLines(),lineInfo);
        }

        private async UniTask SelectLineButtonAsync(CancellationToken ct)
        {
            while (true)
            {
                _beforeSelectLine = _selectingLine;
                _selectingLine = await uiView.LineButtonSelectAsync(ct);
                if (_beforeSelectLine != null)
                {
                    if (_beforeSelectLine == _selectingLine)
                    {
                        _selectingLine = null;
                        _beforeSelectLine.ResetColor();
                        continue;
                    }
                    _beforeSelectLine.ResetColor();
                }
                _selectingLine.SelectColor();
                if (ct.IsCancellationRequested) return;
                
            }
        }
        public async UniTask LineUITaskAsync(CancellationToken ct)
        {
            while (true)
            {
                await uiView.LineMenuButtonOnClickAsync(ct);
                if(_selectingLine != null)_selectingLine.ResetColor();
                await uiView.ShowLineMenu(ct);

                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                var task1 = SelectLineButtonAsync(mergedCts.Token);
                var task2 = uiView.LineMenuBackButtonOnClickAsync(mergedCts.Token);
                await UniTask.WhenAny(task1, task2);
                newCts.Cancel();

                await uiView.HideLineMenu(ct);
                if(_selectingLine != null)_selectingLine.ResetColor();

                if (ct.IsCancellationRequested) return;
            }
        }
    }
}
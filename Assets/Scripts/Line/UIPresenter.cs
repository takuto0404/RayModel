using System.Threading;
using Cysharp.Threading.Tasks;
using Default;
using UnityEngine;

namespace Line
{
    public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
    {
        [SerializeField] private UIView uiView;
        private LineInfoUI _beforeSelect = null;
        private LineInfoUI _selecting = null;

        public void SetMousePointerPosition(Vector2 pos)
        {
            uiView.SetMousePointerPosition(pos);
        }

        public void MakeContents()
        {
            LineInfo lineInfo = null;
            if (_selecting != null)
            {
                lineInfo = _selecting.lineInfo;
            }
            _beforeSelect = uiView.MakeContents(LineManager.Instance.GetAllLines(),lineInfo);
        }

        private async UniTask SelectButtonAsync(CancellationToken ct)
        {
            while (true)
            {
                _beforeSelect = _selecting;
                _selecting = await uiView.ButtonSelectAsync(ct);
                if (_beforeSelect != null)
                {
                    if (_beforeSelect == _selecting)
                    {
                        _selecting = null;
                        _beforeSelect.ResetColor();
                        continue;
                    }
                    _beforeSelect.ResetColor();
                }
                _selecting.SelectColor();
                if (ct.IsCancellationRequested) return;
                
                Debug.Log(_selecting.lineNumberText.text);
            }
        }
        public async UniTask UITaskAsync(CancellationToken ct)
        {
            while (true)
            {
                await uiView.LineMenuButtonOnClickAsync(ct);
                if(_selecting != null)_selecting.ResetColor();
                await uiView.ShowLineMenu(ct);

                var newCts = new CancellationTokenSource();
                var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, newCts.Token);

                var task1 = SelectButtonAsync(mergedCts.Token);
                var task2 = uiView.BackButtonOnClickAsync(mergedCts.Token);
                await UniTask.WhenAny(task1, task2);
                newCts.Cancel();

                await uiView.HideLineMenu(ct);
                if(_selecting != null)_selecting.ResetColor();

                if (ct.IsCancellationRequested) return;
            }
        }
    }
}
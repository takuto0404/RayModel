using System.Threading;
using Cysharp.Threading.Tasks;
using Default;
using UnityEngine;

namespace Line
{
    public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
    {
        [SerializeField] private UIView uiView;

        public void SetMousePointerPosition(Vector2 pos)
        {
            uiView.SetMousePointerPosition(pos);
        }

        public async UniTask UITaskAsync(CancellationToken ct)
        {
            while (true)
            {
                await uiView.LineMenuButtonOnClickAsync(ct);
                await uiView.ShowLineMenu(ct);
                await uiView.BackButtonOnClickAsync(ct);
                await uiView.HideLineMenu(ct);
            }
        }
    }
}
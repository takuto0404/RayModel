using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Default
{
    public class InputProvider : SingletonMonoBehaviour<InputProvider>
    {
        private PlayerInputs _playerInputs;
        public Vector2 mousePosition;
        private bool _clicking = false;
        
        public void SubscribeActions()
        {
            _playerInputs = new PlayerInputs();
        }

        private void OnDestroy()
        {
            _playerInputs.Dispose();
        }

        private async UniTask MouseClickAsync(CancellationToken ct)
        {
            await UniTask.WaitWhile(() => !_clicking, cancellationToken: ct);
            _clicking = false;
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            _clicking = true;
        }

        public void GetMousePosition(InputAction.CallbackContext context)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }
}
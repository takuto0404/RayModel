using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Default
{
    public class InputProvider : SingletonMonoBehaviour<InputProvider>
    {
        private PlayerInputs _playerInputs;
        public Vector2 mousePosition;
        private bool _clicked = false;
        private bool _longPressed = false;
        
        public void SubscribeActions()
        {
            _playerInputs = new PlayerInputs();
        }

        private void OnDestroy()
        {
            _playerInputs.Dispose();
        }

        public async UniTask MouseClickAsync(CancellationToken ct)
        {
            await UniTask.WaitWhile(() => !_clicked, cancellationToken: ct);
            _clicked = false;
        }

        public async UniTask LongPressAsync(CancellationToken ct)
        {
            await UniTask.WaitWhile(() => !_longPressed, cancellationToken: ct);
            _longPressed = false;
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if(context.performed)_clicked = true;
        }
        
        public void OnLongPress(InputAction.CallbackContext context)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if(context.performed)_longPressed = true;
        }

        public void GetMousePosition(InputAction.CallbackContext context)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }
}
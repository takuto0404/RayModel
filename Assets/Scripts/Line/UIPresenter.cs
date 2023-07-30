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
    }
}
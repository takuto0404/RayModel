using UnityEngine;
using UnityEngine.UI;

namespace Line
{
    public class UIView : MonoBehaviour
    {
        [SerializeField] private Image point;

        public void SetMousePointerPosition(Vector2 position)
        {
            point.rectTransform.position = position;
        }
    }
}
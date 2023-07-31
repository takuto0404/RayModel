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
    }
}
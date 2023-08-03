using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Line
{
    public class LineInfoUI : MonoBehaviour
    {
        [SerializeField] public TMP_Text lineNumberText;
        [SerializeField] public TMP_Text lineTypeText;
        [SerializeField] public TMP_Text startPointText;
        [SerializeField] public TMP_Text endPointText;
        [SerializeField] public Button thisButton;
        [SerializeField] private Image thisImage;
        [HideInInspector]public LineInfo lineInfo;

        public void Init(LineInfo newLineInfo,int number)
        {
            lineInfo = newLineInfo;
            lineNumberText.text = number.ToString();
            lineTypeText.text = newLineInfo.LineType.ToString();
            var interval = LineGrid.Instance.interval;
            startPointText.text = $"({newLineInfo.StartPoint.x / interval},{newLineInfo.StartPoint.y / interval})";
            endPointText.text = $"({newLineInfo.EndPoint.x / interval},{newLineInfo.EndPoint.y / interval})";
        }
        public void SelectColor()
        {
            lineInfo.GetUGUILineRenderer().color = Color.magenta;
            thisImage.color = new Color(0.8f,0.5f,0.5f);
        }

        public void ResetColor()
        {
            lineInfo.GetUGUILineRenderer().color = Color.black;
            thisImage.color = Color.white;
        }
    }
}
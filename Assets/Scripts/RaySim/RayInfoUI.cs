using Line;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaySim
{
    public class RayInfoUI : MonoBehaviour
    {
        [SerializeField] public TMP_Text rayNumberText;
        [SerializeField] public TMP_Text startPointText;
        [SerializeField] public TMP_Text vectorText;
        [SerializeField] public Button thisButton;
        [SerializeField] private Image thisImage;
        [HideInInspector]public RayInfo rayInfo;

        public void Init(RayInfo newRayInfo,int number)
        {
            rayInfo = newRayInfo;
            rayNumberText.text = number.ToString();
            vectorText.text = Mathf.Atan2(newRayInfo.Vector.y,newRayInfo.Vector.x).ToString();
            var interval = LineGrid.Instance.interval;
            startPointText.text = $"({newRayInfo.StartPoint.x / interval},{newRayInfo.StartPoint.y / interval})";
        }
        public void SelectColor()
        {
            rayInfo.GetUGUILineRenderer().color = Color.magenta;
            thisImage.color = new Color(0.8f,0.5f,0.5f);
            if (rayInfo.Child.ui != null)
            {
                rayInfo.Child.ui.SelectColor();
            }
        }

        public void ResetColor()
        {
            rayInfo.GetUGUILineRenderer().color = Color.black;
            thisImage.color = Color.white;
            if (rayInfo.Child.ui != null)
            {
                rayInfo.Child.ui.ResetColor();
            }
        }
    }
}
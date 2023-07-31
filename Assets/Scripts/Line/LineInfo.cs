using UnityEngine;

namespace Line
{
    public class LineInfo : MonoBehaviour
    {
        public void Init(Vector2 startPoint, Vector2 endPoint, LineType lineType, MaterialType[] materialTypes)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            LineType = lineType;
            MaterialTypes = materialTypes;
        }

        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; private set; }
        public LineType LineType { get; private set; }
        public MaterialType[] MaterialTypes { get; private set; }
        public UGUILineRenderer LineRenderer => GetComponent<UGUILineRenderer>();

        public void SelectColor()
        {
            LineRenderer.color = Color.magenta;
        }

        public void ResetColor()
        {
            LineRenderer.color = Color.black;
        }
    }
}
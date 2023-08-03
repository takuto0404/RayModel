using Default;
using UnityEngine;

namespace Line
{
    [RequireComponent(typeof(UGUILineRenderer))]
    public class LineInfo : MonoBehaviour,ILineBeAble
    {
        public void Init(Vector2 startPoint, Vector2 endPoint, LineType lineType, MaterialType[] materialTypes)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            LineType = lineType;
            MaterialTypes = materialTypes;
            GetUGUILineRenderer();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public UGUILineRenderer GetUGUILineRenderer()
        {
            if(lineRenderer == null) lineRenderer = GetComponent<UGUILineRenderer>();
            return lineRenderer;
        }

        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; private set; }
        public LineType LineType { get; private set; }
        public MaterialType[] MaterialTypes { get; private set; }
        private UGUILineRenderer lineRenderer;
    }
}
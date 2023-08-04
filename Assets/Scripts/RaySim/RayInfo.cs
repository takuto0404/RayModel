using Default;
using Line;
using UnityEngine;

namespace RaySim
{
    [RequireComponent(typeof(UGUILineRenderer))]
    public class RayInfo : MonoBehaviour,ILineBeAble
    {
        public void Init(Vector2 startPoint, Vector2 vector,Vector2 endPoint)
        {
            StartPoint = startPoint;
            Vector = vector;
            EndPoint = endPoint;
            _lineRenderer = GetUGUILineRenderer();
            Child = (null,null);
            obstacle = null;
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public UGUILineRenderer GetUGUILineRenderer()
        {
            if (_lineRenderer == null) _lineRenderer = GetComponent<UGUILineRenderer>();
            return _lineRenderer;
        }
        
        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; set; }
        public Vector2 Vector { get; private set; }
        private UGUILineRenderer _lineRenderer;
        
        public (RayInfo rayInfo,RayInfoUI ui) Child;
        public LineInfo obstacle;
    }
}
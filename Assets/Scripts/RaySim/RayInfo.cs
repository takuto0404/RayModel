using Default;
using Line;
using UnityEngine;

namespace RaySim
{
    [RequireComponent(typeof(UGUILineRenderer))]
    public class RayInfo : MonoBehaviour,ILineBeAble
    {
        public void Init(Vector2 startPoint, Vector2 vector)
        {
            StartPoint = startPoint;
            Vector = vector;
            EndPoint = startPoint;
            LineRenderer = GetUGUILineRenderer();
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public UGUILineRenderer GetUGUILineRenderer()
        {
            if (LineRenderer == null) LineRenderer = GetComponent<UGUILineRenderer>();
            return LineRenderer;
        }
        
        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; set; }
        public Vector2 Vector { get; private set; }
        private UGUILineRenderer LineRenderer;
    }
}
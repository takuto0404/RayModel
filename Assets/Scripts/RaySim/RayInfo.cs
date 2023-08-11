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
            obstacleId = -1;
        }

        public void DestroyChild(bool destroyThis)
        {
            if (child != null)
            {
                child.DestroyChild(true);
                child = null;
            }
            if(destroyThis)Destroy(gameObject);
        }

        public void SelectColor()
        {
            GetUGUILineRenderer().color = Color.magenta;
            if(child != null)child.SelectColor();
        }

        public void ResetColor()
        {
            GetUGUILineRenderer().color = Color.black;
            if(child != null)child.ResetColor();
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
        
        public RayInfo child;
        public int obstacleId;
    }
}
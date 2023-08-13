using System.Collections.Generic;
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
            isChild = false;
            StartPoint = startPoint;
            Vector = vector;
            EndPoint = endPoint;
            _lineRenderer = GetUGUILineRenderer();
            obstacleId = -1;
            childNest = 1;
        }

        public List<RayInfo> DestroyChild(bool destroyThis)
        {
            List<RayInfo> list;
            if (child == null)
            {
                list = new List<RayInfo>{this};
            }
            else
            {
                list = child.DestroyChild(true);
                child = null;
                obstacleId = -1;
                if(destroyThis)list.Add(this);
            }
            if(destroyThis)Destroy(gameObject);
            return list;
        }

        public void SelectColor()
        {
            GetUGUILineRenderer().color = Color.magenta;
            if(child != null)child.SelectColor();
        }

        public void ResetColor()
        {
            GetUGUILineRenderer().color = Color.red;
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
        public LineInfo ignoreLine;
        
        public RayInfo child;
        public int childNest;
        public bool isChild;
        public int obstacleId;
    }
}
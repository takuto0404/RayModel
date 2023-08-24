using System.Collections.Generic;
using System.Linq;
using Default;
using Line;
using UnityEngine;

namespace RaySim
{
    [RequireComponent(typeof(UGUILineRenderer))]
    public class RayInfo : MonoBehaviour,ILineBeAble
    {
        public void Init(Vector2 startPoint, Vector2 vector,Vector2 endPoint,int color)
        {
            isChild = false;
            StartPoint = startPoint;
            Vector = vector;
            EndPoint = endPoint;
            _lineRenderer = GetUGUILineRenderer();
            obstacleId = -1;
            childNest = 1;
            rayColor = color;
            ResetColor();
        }

        public List<RayInfo> DestroyChild(bool destroyThis)
        {
            List<List<RayInfo>> list;
            if (children == null)
            {
                list = new List<List<RayInfo>>{new (){this}};
            }
            else
            {
                list = children.Select(child => child.DestroyChild(true)).ToList();
                children = null;
                obstacleId = -1;
                if(destroyThis)list.Add(new (){this});
            }

            var newList = new List<RayInfo>();
            list.ForEach(listItem => listItem.ForEach(item => newList.Add(item)));
            if(destroyThis)Destroy(gameObject);
            return newList;
        }

        public void SelectColor()
        {
            GetUGUILineRenderer().color = Color.magenta;
            if(children != null)children.ForEach(child => child.SelectColor());
        }

        public void ResetColor()
        {
            Color color;
            if (rayColor == 0)
            {
                color = Color.red;
            }
            else if (rayColor == 1)
            {
                color = Color.green;
            }
            else
            {
                color = Color.blue;
            }

            color.a = 0.5f;
            GetUGUILineRenderer().color = color;
            if(children != null)children.ForEach(child => child.ResetColor());
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
        public int rayColor;
        public List<RayInfo> children;
        public int childNest;
        public bool isChild;
        public int obstacleId;
    }
}
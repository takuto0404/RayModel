using System.Collections.Generic;
using Default;
using Line;
using UnityEngine;

namespace RaySim
{
    public class RayManager : SingletonMonoBehaviour<RayManager>
    {
        private readonly List<RayInfo> _rayObjects = new();

        public List<RayInfo> GetAllRays()
        {
            return _rayObjects;
        }

        public void CreateRayAsUI(RayInfo rayInfo)
        {
            _rayObjects.Add(rayInfo);
        }

        public void UpdateRayPosition()
        {
            foreach (var ray in _rayObjects)
            {
                var startPos = ray.StartPoint - LineGrid.Instance.totalMisalignment;
                var endPos = ray.EndPoint - LineGrid.Instance.totalMisalignment;
                var viewSize = LineGrid.Instance.viewSize / 2;
                if (Mathf.Abs(endPos.x) < viewSize.x && Mathf.Abs(endPos.y) < viewSize.y)
                {
                    var calculateViewSize = viewSize - new Vector2(Mathf.Abs(endPos.x),Mathf.Abs(endPos.y));
                    Debug.Log(calculateViewSize);
                    var vector = ray.Vector;
                    if (calculateViewSize.x / calculateViewSize.y < Mathf.Abs(vector.x / vector.y))
                    {
                        endPos = vector * Mathf.Abs(calculateViewSize.x / vector.x) + endPos;
                    }
                    else if (calculateViewSize.x / calculateViewSize.y > Mathf.Abs(vector.x / vector.y))
                    {
                        endPos = vector * Mathf.Abs(calculateViewSize.y / vector.y) + endPos;
                    }
                    else
                    {
                        endPos = viewSize;
                    }
                    
                    ray.EndPoint = endPos + LineGrid.Instance.totalMisalignment;
                }

                ray.GetUGUILineRenderer().SetPositions(new[] { startPos, endPos });
            }
        }
    }
}
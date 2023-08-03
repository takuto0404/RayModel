using System;
using System.Collections.Generic;
using Default;
using Line;
using UnityEngine;

namespace RaySim
{
    public class RayManager : SingletonMonoBehaviour<RayManager>
    {
        private readonly List<RayInfo> _rayObjects = new ();

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
                var viewSize = LineGrid.Instance.viewSize;
                if (endPos.x < viewSize.x && endPos.y < viewSize.y)
                {
                    var calculateViewSize = new Vector2(viewSize.x - endPos.x, viewSize.y - endPos.y);
                    var vector = ray.Vector;
                    if (calculateViewSize.x / calculateViewSize.y < vector.x / vector.y)
                    {
                        endPos = new Vector2(viewSize.x, vector.y * (calculateViewSize.x / vector.x) + endPos.y);
                    }
                    else if (calculateViewSize.x / calculateViewSize.y > vector.x / vector.y)
                    {
                        endPos = new Vector2(vector.x * (calculateViewSize.y / vector.y) + endPos.x, viewSize.y);
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
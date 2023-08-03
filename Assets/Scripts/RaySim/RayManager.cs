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
                    var vector = ray.Vector;
                    if (viewSize.x / viewSize.y < vector.x / vector.y)
                    {
                        endPos = new Vector2(viewSize.x, viewSize.y * (vector.x / viewSize.x));
                    }
                    else if (viewSize.x / viewSize.y < vector.x / vector.y)
                    {
                        endPos = viewSize;
                    }
                    else
                    {
                        endPos = new Vector2(viewSize.x * (vector.y / viewSize.y), viewSize.y);
                    }
                }

                ray.EndPoint = endPos;
                ray.LineRenderer.SetPositions(new[] { startPos, endPos });
            }
        }
    }
}
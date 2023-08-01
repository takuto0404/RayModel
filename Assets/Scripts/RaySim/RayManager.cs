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

        public void CreateLineAsUI(RayInfo rayInfo)
        {
            _rayObjects.Add(rayInfo);
        }

        public void UpdateRayPosition()
        {
            foreach (var ray in _rayObjects)
            {
                var startPos = ray.StartPoint - LineGrid.Instance.totalMisalignment;
                var vector = ray.Vector;
                
                ray.LineRenderer.SetPositions(new[] { startPos, endPos });
            }
        }
    }
}
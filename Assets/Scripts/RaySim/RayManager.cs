using System.Collections.Generic;
using Default;
using Line;
using UnityEditor;
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
                var startPos = ray.EndPoint;
                var endPos = ray.EndPoint;
                var vector = ray.Vector;
        
                var max = LineGrid.Instance.maxViewPos;
                var min = LineGrid.Instance.minViewPos;
        
                Vector2 size;
                if (vector.x > 0 && vector.y > 0)
                { 
                    size = max;
                }
                else if (vector.x > 0 && vector.y <= 0)
                {
                    size = new Vector2(max.x, min.y);
                }
                else if(vector.x <= 0 && vector.y > 0)
                {
                    size = new Vector2(min.x, max.y);
                }
                else
                {
                    size = min;
                }
                
                if (Mathf.Abs(endPos.x) < Mathf.Abs(size.x))
                {
                    endPos = startPos + vector * ((size.x - startPos.x) / vector.x);
                    Debug.Log($"xの限界が{size.x}に満たなかったので修正後{endPos.x}になりました。");
                }
                if (Mathf.Abs(endPos.y) < Mathf.Abs(size.y))
                {
                    endPos = startPos + vector * ((size.y - startPos.y) / vector.y);
                    Debug.Log($"yの限界が{size.y}に満たなかったので修正後{endPos.y}になりました。");
                }

                ray.EndPoint = endPos;
        
                ray.GetUGUILineRenderer().SetPositions(new[] { ray.StartPoint - LineGrid.Instance.totalMisalignment, endPos - LineGrid.Instance.totalMisalignment});
            }
        }
    }
}
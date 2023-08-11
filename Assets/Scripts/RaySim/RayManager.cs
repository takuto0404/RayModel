using System;
using System.Collections.Generic;
using Default;
using Line;
using UnityEditor;
using UnityEngine;
using LineInfo = Line.LineInfo;

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
                }
                if (Mathf.Abs(endPos.y) < Mathf.Abs(size.y))
                {
                    endPos = startPos + vector * ((size.y - startPos.y) / vector.y);
                }
                ray.EndPoint = endPos;
                
                var wallResult = SearchWall(ray);
                if (wallResult.obstacle != null)
                {
                    var line = wallResult.obstacle;
                    endPos = wallResult.pos;
                    ray.EndPoint = wallResult.pos;
                    ray.obstacle = line;

                    if (line.LineType == LineType.Mirror)
                    {
                        //うまくいけばここもっと省略できそう
                        Vector2 normal;
                        var lineVector = line.EndPoint - line.StartPoint;
                        if ((int)Mathf.Sign(lineVector.x) == (int)Mathf.Sign(lineVector.y))
                        {
                            if (lineVector.x / lineVector.y < ray.Vector.x / ray.Vector.y)
                            {
                                normal = new Vector2(Mathf.Abs(lineVector.x), -Mathf.Abs(lineVector.y));
                            }
                            else
                            {
                                normal = new Vector2(-Mathf.Abs(lineVector.x), Mathf.Abs(lineVector.y));
                            }
                        }
                        else
                        {
                            if ((int)Mathf.Sign(lineVector.x) == -1)
                            {
                                lineVector *= new Vector2(-1, -1);
                            }
                            if (lineVector.x / lineVector.y < ray.Vector.x / ray.Vector.y)
                            {
                                normal = new Vector2(Mathf.Abs(lineVector.x), -Mathf.Abs(lineVector.y));
                            }
                            else
                            {
                                normal = new Vector2(-Mathf.Abs(lineVector.x), Mathf.Abs(lineVector.y));
                            }
                        }

                        normal /= (Vector2.zero - normal).magnitude;
                        var reflect = Vector2.Reflect(ray.Vector, normal);
                        Debug.Log(reflect);
                    }
                }
                
        
                ray.GetUGUILineRenderer().SetPositions(new[] { ray.StartPoint - LineGrid.Instance.totalMisalignment, endPos - LineGrid.Instance.totalMisalignment});
            }
        }

        private (LineInfo obstacle,Vector2 pos) SearchWall(RayInfo rayInfo)
        {
            (LineInfo lineInfo, Vector2 pos) mostNearObstacle = (null,Vector2.zero);
            var lines = LineManager.Instance.GetAllLines();
            foreach (var line in lines)
            {
                if (MyMath.Calculator.IsLineIntersected(line.StartPoint, line.EndPoint, rayInfo.StartPoint,
                        rayInfo.EndPoint))
                {
                    continue;
                }

                var pos = MyMath.Calculator.LineIntersection(line.StartPoint, line.EndPoint, rayInfo.StartPoint,
                    rayInfo.EndPoint);
                var x = Mathf.Max(line.StartPoint.x, line.EndPoint.x) >= pos.x &&
                        pos.x >= Math.Min(line.StartPoint.x, line.EndPoint.x);
                var y = Mathf.Max(line.StartPoint.y, line.EndPoint.y) >= pos.y &&
                        pos.y >= Math.Min(line.StartPoint.y, line.EndPoint.y);
                if (!x || !y)
                {
                    continue;
                }

                var rayMinusOrPlus = (rayInfo.Vector.x >= 0, rayInfo.Vector.y >= 0);
                var posMinusOrPlus = (pos.x - rayInfo.StartPoint.x > 0,pos.y - rayInfo.StartPoint.y > 0);
                if (rayMinusOrPlus != posMinusOrPlus)
                {
                    continue;
                }
                
                if (mostNearObstacle.lineInfo == null)
                {
                    mostNearObstacle = (line,pos);
                    continue;
                }

                var beforeDistance = (mostNearObstacle.pos - rayInfo.StartPoint).magnitude;
                var distance = (pos - rayInfo.StartPoint).magnitude;
                if (distance < beforeDistance)
                {
                    mostNearObstacle = (line,pos);
                }
            }

            return (mostNearObstacle.lineInfo, mostNearObstacle.pos);
        }
    }
}
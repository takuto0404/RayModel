using System;
using System.Collections.Generic;
using Default;
using Line;
using UI;
using UnityEditor;
using UnityEngine;
using LineInfo = Line.LineInfo;

namespace RaySim
{
    public class RayManager : SingletonMonoBehaviour<RayManager>
    {
        private readonly List<RayInfo> _rayObjects = new();
        [SerializeField] private RayInfo rayPrefab;
        [SerializeField] private Transform canvasTransform;
        private List<RayInfo> _reserve = new();
        private List<RayInfo> _destroyReserve = new();

        public List<RayInfo> GetAllRays()
        {
            return _rayObjects;
        }

        public void CreateRay(RayInfo rayInfo)
        {
            _rayObjects.Add(rayInfo);
        }
        

        public void UpdateRayPosition(int a)
        {
            if (a > 20)
            {
                Debug.Log("A");
                return;
            }

            foreach (var ray in _rayObjects)
            {
                var startPos = ray.EndPoint;
                var endPos = ray.EndPoint;
                var vector = ray.Vector;

                if (ray.obstacleId == -1)
                {

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
                    else if (vector.x <= 0 && vector.y > 0)
                    {
                        size = new Vector2(min.x, max.y);
                    }
                    else
                    {
                        size = min;
                    }

                    if (Mathf.Abs(endPos.x) < Mathf.Abs(size.x))
                    {
                        if (ray.Vector.x != 0)
                        {
                            endPos = startPos + vector * ((size.x - startPos.x) / vector.x);
                        }
                    }

                    if (Mathf.Abs(endPos.y) < Mathf.Abs(size.y))
                    {
                        if (ray.Vector.y != 0)
                        {
                            endPos = startPos + vector * ((size.y - startPos.y) / vector.y);
                        }
                    }

                    ray.EndPoint = endPos;
                }
                var obstacle = SearchWall(ray);
                if (obstacle.obstacle != null)
                {
                    var line = obstacle.obstacle;
                    if (ray.obstacleId != -1 && ray.obstacleId != line.Id)
                    {
                        var destroyed = ray.DestroyChild(false);
                        
                        destroyed.ForEach(_destroyReserve.Add);
                        break;
                    }
                    if(ray.child == null)
                    {
                        ray.EndPoint = obstacle.pos;
                        var isPlusPlusAngle =
                            (line.StartPoint.x > line.EndPoint.x && line.StartPoint.y > line.EndPoint.y) ||
                            (line.EndPoint.x > line.StartPoint.x && line.EndPoint.y > line.StartPoint.y);
                        var lineVector = line.EndPoint - line.StartPoint;
                        if (lineVector.x < 0) lineVector *= -1;
                        var normal = new Vector2(Mathf.Abs(lineVector.y),Mathf.Abs(lineVector.x));
                        var lineAngle = Mathf.Atan2(lineVector.x,lineVector.y);
                        var rayAngle = Mathf.Atan2(ray.Vector.x, ray.Vector.y);
                        if (isPlusPlusAngle)
                        {
                            if (lineAngle < rayAngle)
                            {
                                normal *= new Vector2(1, -1);
                            }
                            else
                            {
                                normal *= new Vector2(-1, 1);
                            }
                        }
                        else if((int)line.StartPoint.x == (int)line.EndPoint.x)
                        {
                            if (ray.StartPoint.y > obstacle.pos.y)
                            {
                                normal = Vector2.left;
                            }
                            else
                            {
                                normal = Vector2.right;
                            }
                        }
                        else if ((int)line.StartPoint.y == (int)line.EndPoint.y)
                        {
                            if (ray.StartPoint.x > obstacle.pos.x)
                            {
                                normal = Vector2.up;
                            }
                            else
                            {
                                normal = Vector2.down;
                            }
                        }
                        else
                        {
                            if (lineAngle < rayAngle)
                            {
                                normal *= new Vector2(1, 1);
                            }
                            else
                            {
                                normal *= new Vector2(-1, -1);
                            }
                        }
                        normal /= (normal - Vector2.zero).magnitude;

                        if (line.LineType == LineType.Mirror)
                        {
                            var reflect = Vector2.Reflect(ray.Vector, normal);
                            var newRay = Instantiate(rayPrefab, canvasTransform);
                            newRay.Init(ray.EndPoint,reflect,ray.EndPoint + reflect);
                            ray.child = newRay;
                            ray.obstacleId = line.Id;
                            _reserve.Add(newRay);
                            break;
                        }
                    }
                }
                else
                {
                    if (ray.obstacleId != -1)
                    {
                        var destroyed = ray.DestroyChild(false);
                        
                        destroyed.ForEach(_destroyReserve.Add);
                        break;
                    }
                }
                
                ray.GetUGUILineRenderer().SetPositions(new[]
                {
                    ray.StartPoint - LineGrid.Instance.totalMisalignment, endPos - LineGrid.Instance.totalMisalignment
                });
            }

            if (_reserve.Count > 0)
            {
                _reserve.ForEach(CreateRay);
                _reserve = new();
                UIPresenter.Instance.MakeRayContents();
                UpdateRayPosition(a + 1);
            }

            if (_destroyReserve.Count > 0)
            {
                _destroyReserve.ForEach(item => _rayObjects.Remove(item));
                _destroyReserve = new();
            }
        }
        private (LineInfo obstacle, Vector2 pos) SearchWall(RayInfo rayInfo)
        {
            (LineInfo lineInfo, Vector2 pos) mostNearObstacle = (null, Vector2.zero);
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
                if (pos == rayInfo.StartPoint) continue;
                var x = Mathf.Max(line.StartPoint.x, line.EndPoint.x) >= pos.x &&
                        pos.x >= Math.Min(line.StartPoint.x, line.EndPoint.x);
                var y = Mathf.Max(line.StartPoint.y, line.EndPoint.y) >= pos.y &&
                        pos.y >= Math.Min(line.StartPoint.y, line.EndPoint.y);
                if (!x || !y)
                {
                    continue;
                }
                
                var rayMinusOrPlus = (rayInfo.Vector.x >= 0, rayInfo.Vector.y >= 0);
                var posMinusOrPlus = (pos.x - rayInfo.StartPoint.x >= 0, pos.y - rayInfo.StartPoint.y >= 0);
                if (rayMinusOrPlus != posMinusOrPlus)
                {
                    continue;
                }
                
                if (mostNearObstacle.lineInfo == null)
                {
                    mostNearObstacle = (line, pos);
                    continue;
                }

                var beforeDistance = (mostNearObstacle.pos - rayInfo.StartPoint).magnitude;
                var distance = (pos - rayInfo.StartPoint).magnitude;
                if (distance < beforeDistance)
                {
                    mostNearObstacle = (line, pos);
                }
            }

            return (mostNearObstacle.lineInfo, mostNearObstacle.pos);
        }
    }
}
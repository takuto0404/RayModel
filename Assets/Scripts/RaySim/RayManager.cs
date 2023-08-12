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
        private int roopNum = 0;

        public List<RayInfo> GetAllRays()
        {
            return _rayObjects;
        }

        public void CreateRay(RayInfo rayInfo)
        {
            _rayObjects.Add(rayInfo);
        }

        private bool IsNull(GameObject var)
        {
            var go = var;
            return go == null;
        }

        public void UpdateRayPosition(int a)
        {
            if (a > 2)
            {
                Debug.Log("A");
                return;
            }

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

                var wallResult = SearchWall(ray);
                if (wallResult.obstacle != null)
                {
                    if (wallResult.obstacle.Id != ray.obstacleId)
                    {
                        ray.DestroyChild(false);
                    }

                    var line = wallResult.obstacle;
                    endPos = wallResult.pos;
                    ray.EndPoint = wallResult.pos;
                    ray.obstacleId = line.Id;

                    if (ray.child == null)
                    {
                        if (line.LineType == LineType.Mirror)
                        {
                            var lineVector = line.EndPoint - line.StartPoint;
                            Vector2 normal = new Vector2(lineVector.y,lineVector.x);

                            if (startPos.x < endPos.x)
                            {
                                normal.x *= -1;
                            }

                            if (startPos.y < endPos.y)
                            {
                                normal.y *= -1;
                            }

                            normal /= (Vector2.zero - normal).magnitude;

                            var reflect = Vector2.Reflect(ray.Vector, normal);

                            var newRay = Instantiate(rayPrefab, Vector2.zero, Quaternion.identity, canvasTransform);
                            newRay.Init(ray.EndPoint, reflect, ray.EndPoint - reflect);
                            Debug.Log(reflect == newRay.Vector);
                            ray.child = newRay;
                            _reserve.Add(newRay);
                        }
                    }
                }
                else
                {
                    if (ray.obstacleId != -1)
                    {
                        var destroyed = ray.DestroyChild(false);
                        destroyed.ForEach(item => _rayObjects.Remove(item));
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
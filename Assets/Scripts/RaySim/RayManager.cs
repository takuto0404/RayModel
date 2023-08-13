using System;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<MaterialType, float> _refractiveIndex = new Dictionary<MaterialType, float>()
        {
            { MaterialType.Water ,1.333f},
            { MaterialType.Air ,1.000f}
        };

        public List<RayInfo> GetAllParentRays()
        {
            return _rayObjects.Where(ray => !ray.isChild).ToList();
        }

        public void CreateRay(RayInfo rayInfo)
        {
            _rayObjects.Add(rayInfo);
        }

        private void UpdateRayPosition(RayInfo ray)
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
            if (obstacle.obstacle != null && ray.ignoreLine != obstacle.obstacle)
            {
                var line = obstacle.obstacle;
                if (ray.obstacleId != -1 && ray.obstacleId != line.Id)
                {
                    var destroyed = ray.DestroyChild(false);

                    destroyed.ForEach(_destroyReserve.Add);
                }

                if (ray.child == null)
                {
                    ray.EndPoint = obstacle.pos;

                    var isPlusPlusAngle =
                        (line.StartPoint.x > line.EndPoint.x && line.StartPoint.y > line.EndPoint.y) ||
                        (line.EndPoint.x > line.StartPoint.x && line.EndPoint.y > line.StartPoint.y);
                    var lineVector = line.EndPoint - line.StartPoint;
                    if (lineVector.x < 0) lineVector *= -1;

                    var normal = new Vector2(Mathf.Abs(lineVector.y), Mathf.Abs(lineVector.x));
                    var lineAngle = Mathf.Atan2(lineVector.y, lineVector.x);
                    var rayAngle = Mathf.Atan2(ray.Vector.y, ray.Vector.x);
                    var isIn = false;
                    if (isPlusPlusAngle)
                    {
                        if (lineAngle < rayAngle)
                        {
                            normal *= new Vector2(1, -1);
                        }
                        else
                        {
                            normal *= new Vector2(-1, 1);
                            isIn = true;
                        }
                    }
                    else if ((int)line.StartPoint.x == (int)line.EndPoint.x)
                    {
                        if (ray.StartPoint.x > obstacle.pos.x)
                        {
                            normal = Vector2.right;
                            isIn = true;
                        }
                        else
                        {
                            normal = Vector2.left;
                        }
                    }
                    else if ((int)line.StartPoint.y == (int)line.EndPoint.y)
                    {
                        if (ray.StartPoint.y > obstacle.pos.y)
                        {
                            normal = Vector2.up;
                            isIn = true;
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
                            isIn = true;
                        }
                    }

                    normal /= (normal - Vector2.zero).magnitude;

                    
                    
                    if (line.LineType == LineType.Mirror)
                    {
                        var reflect = Vector2.Reflect(ray.Vector, normal);
                        var newRay = Instantiate(rayPrefab, canvasTransform);
                        newRay.Init(ray.EndPoint, reflect, ray.EndPoint + reflect);
                        newRay.ignoreLine = line;
                        ray.child = newRay;
                        newRay.isChild = true;
                        newRay.childNest = ray.childNest + 1;
                        if (newRay.childNest > 40)
                        {
#if UNITY_EDITOR
    Debug.Log("反射が複雑になりすぎたため強制終了します。");                        
    EditorApplication.isPlaying = false;
#else
    Application.Quit();//ゲームプレイ終了
#endif
                        }
                        ray.obstacleId = line.Id;
                        _reserve.Add(newRay);
                    }
                    else
                    {
                        var refract = Refract(ray.Vector, normal, _refractiveIndex[line.MaterialTypes[0]],_refractiveIndex[line.MaterialTypes[1]]);
                        var newRay = Instantiate(rayPrefab, canvasTransform);
                        newRay.Init(ray.EndPoint, refract, ray.EndPoint + refract);
                        newRay.ignoreLine = line;
                        ray.child = newRay;
                        newRay.isChild = true;
                        newRay.childNest = ray.childNest + 1;
                        if (newRay.childNest > 40)
                        {
#if UNITY_EDITOR
    Debug.Log("屈折が複雑になりすぎたため強制終了します。");                        
    EditorApplication.isPlaying = false;
#else
    Application.Quit();//ゲームプレイ終了
#endif
                        }
                        ray.obstacleId = line.Id;
                        _reserve.Add(newRay);
                    }
                }
                if(ray.child != null)UpdateRayPosition(ray.child);
            }
            else
            {
                if (ray.obstacleId != -1)
                {
                    var destroyed = ray.DestroyChild(false);

                    destroyed.ForEach(_destroyReserve.Add);
                }
            }

            ray.GetUGUILineRenderer().SetPositions(new[]
            {
                ray.StartPoint - LineGrid.Instance.totalMisalignment, endPos - LineGrid.Instance.totalMisalignment
            });
        }

        [MenuItem("Original/A")]
        private static void A()
        {
            Instance.Refract(new Vector2(1,-1), Vector2.down, 1.333f, 1.0f);
        }
        
        private Vector2 Refract(Vector2 inDirection, Vector2 inNormal,float refractiveIndex1,float refractiveIndex2)
        {
            var inDirectionAngle = Mathf.Atan2(inDirection.y,inDirection.x);
            var normalAngle = Mathf.Atan2(inNormal.y, inNormal.x);
            Debug.Log(inDirectionAngle);
            Vector2 plusMinus;
            var diff = (inDirectionAngle - normalAngle) * Mathf.Rad2Deg;
            if (diff < -180) diff %= -180;
            if (diff >= 180) diff %= 180;
            if (diff > 90)
            {
                Debug.Log("A");
                inDirectionAngle = 180 * Mathf.Deg2Rad - inDirectionAngle;
                plusMinus = new Vector2(-1, -1);
            }
            else if (diff > 0)
            {
                plusMinus = new Vector2(1, -1);
            }
            else if (diff > -90)
            {
                plusMinus = new Vector2(1, 1);
            }
            else
            {
                inDirectionAngle = -180 * Mathf.Deg2Rad - inDirectionAngle;
                plusMinus = new Vector2(-1, 1);
            }
            var incidenceAngle = -inDirectionAngle - normalAngle;
            var isMinus = false;
            if (incidenceAngle < 0)
            {
                incidenceAngle *= -1;
                isMinus = true;
            }
            var refractionAngle = Mathf.Asin(refractiveIndex1 / refractiveIndex2 * Mathf.Sin(incidenceAngle)) * Mathf.Rad2Deg;
            if (float.IsNaN(refractionAngle))
            {
                return Vector2.Reflect(inDirection, inNormal);
            }
            
            var outDirectionAngle = -normalAngle * Mathf.Rad2Deg - refractionAngle;
            var outVector = AngleToVector(outDirectionAngle);
            if (isMinus) outVector *= -1;
            outVector *= plusMinus;
            Debug.Log($"{inDirectionAngle * Mathf.Rad2Deg}度で入射、{normalAngle * Mathf.Rad2Deg}度の法線ベクトルで、{incidenceAngle * Mathf.Rad2Deg}度の入射角、{refractionAngle}どの屈折角、{outDirectionAngle}={outVector}方向に屈折します。");
            return outVector;
        }

        private Vector2 AngleToVector(float angleDegrees)
        {
            var x = Mathf.Cos(angleDegrees * Mathf.Deg2Rad);
            var y = Mathf.Sin(angleDegrees * Mathf.Deg2Rad);
            return new Vector2(x, y);
        }
        public void UpdateRaysPosition()
        {
            foreach (var ray in GetAllParentRays())
            {
                UpdateRayPosition(ray);
            }

            if (_reserve.Count > 0)
            {
                _reserve.ForEach(CreateRay);
                _reserve = new();
                UIPresenter.Instance.MakeRayContents();
                UpdateRaysPosition();
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
                var x = Mathf.Max(line.StartPoint.x, line.EndPoint.x) > pos.x &&
                        pos.x > Math.Min(line.StartPoint.x, line.EndPoint.x) ||
                        (int)line.StartPoint.x == (int)line.EndPoint.x;
                var y = Mathf.Max(line.StartPoint.y, line.EndPoint.y) > pos.y &&
                        pos.y > Math.Min(line.StartPoint.y, line.EndPoint.y) ||
                        (int)line.StartPoint.y == (int)line.EndPoint.y;
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
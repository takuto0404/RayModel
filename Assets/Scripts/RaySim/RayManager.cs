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

        private readonly Dictionary<int, Dictionary<MaterialType, float>> _refractiveIndex = new()
        {
            {
                0, new Dictionary<MaterialType, float>()
                {
                    // { MaterialType.Water, 1.338f },
                    { MaterialType.Water, 1.5f },
                    { MaterialType.Air, 1.000f },
                    { MaterialType.Oil, 1.408f }
                }
            },
            {
                1, new Dictionary<MaterialType, float>()
                {
                    // { MaterialType.Water, 1.332f },
                    { MaterialType.Water, 1.3f },
                    { MaterialType.Air, 1.000f },
                    { MaterialType.Oil, 1.400f }
                }
            },
            {
                2, new Dictionary<MaterialType, float>()
                {
                    // { MaterialType.Water, 1.331f },
                    { MaterialType.Water, 1.1f },
                    { MaterialType.Air, 1.000f },
                    { MaterialType.Oil, 1.397f }
                }
            }
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

                if (ray.children.Count == 0)
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
                            Debug.Log("Air");
                        }
                        else
                        {
                            normal = Vector2.left;
                            Debug.Log("Water");
                        }
                    }
                    else if ((int)line.StartPoint.y == (int)line.EndPoint.y)
                    {
                        if (ray.StartPoint.y > obstacle.pos.y)
                        {
                            normal = Vector2.up;
                        }
                        else
                        {
                            normal = Vector2.down;
                            isIn = true;
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
                        var newRay = CreateNewMirrorRay(ray, line, normal, ray.rayColor);
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
                    else if (line.LineType == LineType.Boundary)
                    {
                        (bool isRefract, Vector2 refract) result;
                        if (isIn)
                        {
                            result = Refract(ray.Vector, normal, _refractiveIndex[ray.rayColor][line.MaterialTypes[0]],
                                _refractiveIndex[ray.rayColor][line.MaterialTypes[1]]);
                        }
                        else
                        {
                            result = Refract(ray.Vector, normal, _refractiveIndex[ray.rayColor][line.MaterialTypes[1]],
                                _refractiveIndex[ray.rayColor][line.MaterialTypes[0]]);
                        }

                        if (result.isRefract)
                        {
                            var refract = result.refract;
                            var newRay = CreateNewBoundaryRay(ray, line, refract, ray.rayColor);
                            var color = newRay.GetUGUILineRenderer().color;
                            newRay.GetUGUILineRenderer().color = color;
                            if (newRay.childNest > 40)
                            {
#if UNITY_EDITOR
                                Debug.Log("屈折が複雑になりすぎたため強制終了します。");
                                EditorApplication.isPlaying = false;
#else
    Application.Quit();//ゲームプレイ終了
#endif
                            }

                            _reserve.Add(newRay);
                        }

                        var newMirrorRay = CreateNewMirrorRay(ray, line, normal, ray.rayColor);
                        var colour = newMirrorRay.GetUGUILineRenderer().color;
                        newMirrorRay.GetUGUILineRenderer().color = colour;
                        if (line.LineType == LineType.Boundary && result.isRefract)
                        {
                            newMirrorRay.GetUGUILineRenderer().color = colour;
                        }

                        if (newMirrorRay.childNest > 40)
                        {
#if UNITY_EDITOR
                            Debug.Log("反射が複雑になりすぎたため強制終了します。");
                            EditorApplication.isPlaying = false;
#else
    Application.Quit();//ゲームプレイ終了
#endif
                        }

                        _reserve.Add(newMirrorRay);
                        ray.obstacleId = line.Id;
                    }
                }

                if (ray.children != null) ray.children.ForEach(UpdateRayPosition);
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

        private RayInfo CreateNewMirrorRay(RayInfo ray, LineInfo line, Vector2 normal, int color)
        {
            var reflect = Vector2.Reflect(ray.Vector, normal);
            var newRay = Instantiate(rayPrefab, canvasTransform);
            newRay.Init(ray.EndPoint, reflect, ray.EndPoint + reflect, color);
            newRay.ignoreLine = line;
            ray.children.Add(newRay);
            newRay.isChild = true;
            newRay.childNest = ray.childNest + 1;
            return newRay;
        }

        private RayInfo CreateNewBoundaryRay(RayInfo ray, LineInfo line, Vector2 refract, int color)
        {
            var newRay = Instantiate(rayPrefab, canvasTransform);
            newRay.Init(ray.EndPoint, refract, ray.EndPoint + refract, color);
            newRay.ignoreLine = line;
            ray.children.Add(newRay);
            newRay.isChild = true;
            newRay.childNest = ray.childNest + 1;
            return newRay;
        }

        private (bool isRefract, Vector2 refract) Refract(Vector2 inDirection, Vector2 inNormal,
            float refractiveIndexIn,
            float refractiveIndexOut)
        {
            var inDirectionAngle = Mathf.Atan2(inDirection.y, inDirection.x) * Mathf.Rad2Deg;

            var inNormalAngle = Mathf.Atan2(inNormal.y, inNormal.x) * Mathf.Rad2Deg;

            var rotatedInDirectionAngle = inDirectionAngle + (-90 - inNormalAngle);
            var isMoreThan90 = rotatedInDirectionAngle > 90;
            var incidence = Mathf.Abs(rotatedInDirectionAngle - 90);

            var refractAngle =
                Mathf.Asin(refractiveIndexIn / refractiveIndexOut * Mathf.Sin(incidence * Mathf.Deg2Rad)) *
                Mathf.Rad2Deg;
            if (double.IsNaN(refractAngle))
            {
                return (false, Vector2.zero);
            }

            float rotatedRefractAngle;
            var oppositeNormalVector = inNormal * new Vector2(-1, -1);
            var oppositeNormal = Mathf.Atan2(oppositeNormalVector.y, oppositeNormalVector.x) * Mathf.Rad2Deg;
            if (oppositeNormal < 0)
            {
                oppositeNormal = 180 - (-180 - oppositeNormal);
            }

            if (isMoreThan90)
            {
                rotatedRefractAngle = oppositeNormal + refractAngle;
            }
            else
            {
                rotatedRefractAngle = oppositeNormal - refractAngle;
            }

            rotatedRefractAngle %= 360;


            return (true, AngleToVector(rotatedRefractAngle));
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
            Debug.Log(_rayObjects.Count);
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
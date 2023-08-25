using System;
using System.Collections.Generic;
using Default;
using TMPro;
using UnityEngine;

namespace Line
{
    public class LineManager : SingletonMonoBehaviour<LineManager>
    {
        private readonly Dictionary<LineType, List<LineInfo>> _lineObjects = new();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            Instance.InstanceInit();
        }

        public List<LineInfo> GetAllLines()
        {
            var list = new List<LineInfo>();
            foreach (var lineInfos in _lineObjects.Values)
            {
                foreach (var lineInfo in lineInfos)
                {
                    list.Add(lineInfo);
                }
            }

            return list;
        }

        private void InstanceInit()
        {
            foreach (var lineType in (LineType[])Enum.GetValues(typeof(LineType)))
            {
                _lineObjects.Add(lineType, new List<LineInfo>());
            }
        }

        public void CreateLineAsUI(LineInfo lineInfo)
        {
            _lineObjects[lineInfo.LineType].Add(lineInfo);
        }

        public void UpdateLinePosition()
        {
            foreach (var lineInfos in _lineObjects.Values)
            {
                foreach (var lineInfo in lineInfos)
                {
                    var startPos = lineInfo.StartPoint - LineGrid.Instance.totalMisalignment;
                    var endPos = lineInfo.EndPoint - LineGrid.Instance.totalMisalignment;
                    lineInfo.GetUGUILineRenderer().SetPositions(new[] { startPos, endPos });

                    if (lineInfo.LineType == LineType.Boundary)
                    {
                        var vector = startPos - endPos;
                        var angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
                        if (angle > 90)
                        {
                            angle = -90 + (angle - 90);
                        }
                        else if (angle < -90)
                        {
                            angle = 90 - (-90 - angle);
                        }

                        var p = vector / vector.magnitude * 30;
                        if (p.x < 0) p *= new Vector2(-1, -1);
                        var textVector = new Vector2(p.y,p.x);
                        var textPos = startPos - vector / 2;
                        if (angle > 0)
                        {
                            lineInfo.SetMaterialText(angle,textPos + textVector,textPos - textVector);
                        }
                        else
                        {
                            lineInfo.SetMaterialText(angle,textPos - textVector,textPos + textVector);
                        }
                    }
                }
            }
        }
    }
}
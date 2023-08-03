using System;
using System.Collections.Generic;
using Default;
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
                }
            }
        }
    }
}
using UnityEngine;

namespace Line
{
    public class LineInfo
    {
        public LineInfo(Vector2 startPoint,Vector2 endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public Vector2 StartPoint { get; }
        public Vector2 EndPoint { get; }
    }
}
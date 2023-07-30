using System.Collections;
using System.Collections.Generic;
using Line;
using UnityEngine;

namespace Math
{
    public static class Calculator
    {
        public static bool IsLineIntersected(LineInfo line1,LineInfo line2)
        {
            var t1 = f(line1.StartPoint, line1.EndPoint, line2.StartPoint);
            var t2 = f(line1.StartPoint, line1.EndPoint, line2.EndPoint);
            var t3 = f(line2.StartPoint, line2.EndPoint, line1.StartPoint);
            var t4 = f(line2.StartPoint, line2.EndPoint, line1.EndPoint);
            return t1 * t2 < 0 && t3 * t4 < 0;
        
            float f(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p2.x - p1.y) * (p3.x - p1.y) - (p2.x - p1.y) * (p3.x - p1.y);
            }
        }
        public static Vector2 LineIntersection(LineInfo line1, LineInfo line2)
        {
            var det = (line1.StartPoint.x - line1.EndPoint.x) * (line2.EndPoint.y - line2.StartPoint.y) - (line2.EndPoint.x - line2.StartPoint.x) * (line1.StartPoint.y- line1.EndPoint.y);
            var t = ((line2.EndPoint.y - line2.StartPoint.y) * (line2.EndPoint.x - line1.EndPoint.x) + (line2.StartPoint.x - line2.EndPoint.x) * (line2.EndPoint.y - line1.EndPoint.y)) / det;
            var x = t * line1.StartPoint.x + (1.0f - t) * line1.EndPoint.x + 0.5f;
            var y = t * line1.StartPoint.y + (1.0f - t) * line1.EndPoint.y + 0.5f;
            return new Vector2((int)x, (int)y);
        }
    }
}


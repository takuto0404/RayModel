using Line;
using UnityEngine;

namespace MyMath
{
    public static class Calculator
    {
        public static bool IsLineIntersected(Vector2 pos1,Vector2 pos2,Vector2 pos3,Vector2 pos4)
        {
            var t1 = F(pos1, pos2, pos3);
            var t2 = F(pos1, pos2, pos4);
            var t3 = F(pos3, pos4, pos1);
            var t4 = F(pos3, pos4, pos2);
            return t1 * t2 < 0 && t3 * t4 < 0;
        
            float F(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p2.x - p1.y) * (p3.x - p1.y) - (p2.x - p1.y) * (p3.x - p1.y);
            }
        }
        public static Vector2 LineIntersection(Vector2 pos1,Vector2 pos2,Vector2 pos3,Vector2 pos4)
        {
            var det = (pos1.x - pos2.x) * (pos4.y - pos3.y) - (pos4.x - pos3.x) * (pos1.y- pos2.y);
            var t = ((pos4.y - pos3.y) * (pos4.x - pos2.x) + (pos3.x - pos4.x) * (pos4.y - pos2.y)) / det;
            var x = t * pos1.x + (1.0f - t) * pos2.x + 0.5f;
            var y = t * pos1.y + (1.0f - t) * pos2.y + 0.5f;
            return new Vector2((int)x, (int)y);
        }
    }
}


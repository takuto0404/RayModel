using Line;
using UnityEngine;

namespace RaySim
{
    public class RayInfo : MonoBehaviour
    {
        public void Init(Vector2 startPoint, Vector2 vector)
        {
            StartPoint = startPoint;
            Vector = vector;
        }
        
        public Vector2 StartPoint { get; private set; }
        public Vector2 Vector { get; private set; }
        public UGUILineRenderer LineRenderer => GetComponent<UGUILineRenderer>();
    }
}
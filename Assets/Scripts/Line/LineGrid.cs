using System.Collections.Generic;
using System.Linq;
using Default;
using UnityEngine;

namespace Line
{
    public class LineGrid : SingletonMonoBehaviour<LineGrid>
    {
        private Vector3[] _verts;
        private int[] _triangles;
        
        private List<UGUILineRenderer> _horizontalLines;
        private List<UGUILineRenderer> _verticalLines;

        private Vector2 _misalignment;
        [HideInInspector]public Vector2 totalMisalignment;
        
        [SerializeField] private UGUILineRenderer lineRenderer;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private float interval = 100;
        [SerializeField] public Vector2 viewSize = new(1920, 1080);
        private Vector2 _farIntegerSize;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            Instance.CreateGrids();
        }

        public Vector2 GetMousePoint()
        {
            var mousePos = InputProvider.Instance.mousePosition;
            var minusMisalignment = new Vector2(interval - _misalignment.x, interval - _misalignment.y);
            var gridPos = new Vector2(Mathf.Round((mousePos.x - minusMisalignment.x) / interval) * interval + minusMisalignment.x,
                Mathf.Round((mousePos.y - minusMisalignment.y) / interval) * interval + minusMisalignment.y);
            return gridPos;
        }

        private void CreateGrids()
        {
            _misalignment = new Vector2();
            totalMisalignment = new Vector2();
            _horizontalLines = new List<UGUILineRenderer>();
            _verticalLines = new List<UGUILineRenderer>();
            _farIntegerSize = new Vector2(FarInteger(viewSize.x / interval), FarInteger(viewSize.y / interval));
            for (var i = 0; i < _farIntegerSize.x + 2; i++)
            {
                var instantiated = Instantiate(lineRenderer,canvasTransform);
                var x = interval * (i - 1) - viewSize.x / 2f;
                instantiated.SetPositions(new Vector2[]{new (x,-viewSize.y / 2f),new (x, viewSize.y / 2f)});
                _horizontalLines.Add(instantiated);
            }
            for (var i = 0; i < _farIntegerSize.y + 2; i++)
            {
                var instantiated = Instantiate(lineRenderer,canvasTransform);
                var y = interval * (i - 1) - viewSize.y / 2f;
                instantiated.SetPositions(new Vector2[]{new (-viewSize.x / 2f,y),new (viewSize.x / 2f,y)});
                _verticalLines.Add(instantiated);
            }
        }
        private int FarInteger(float num)
        {
            return (int)(num - num % 1) + 1;
        }

        public void UpdateGrid(Vector2 move)
        {
            var vector2 = _misalignment - move;
            vector2.x %= interval;
            vector2.y %= interval;
            var difference = _misalignment - vector2;
            _misalignment = vector2;
            totalMisalignment -= move;

            foreach(var line in _horizontalLines)
            {
                var positions = line.positions;
                var newPositions = positions.Select(pos => new Vector2(pos.x + difference.x, pos.y)).ToArray();
                line.SetPositions(newPositions);
            }
            foreach(var line in _verticalLines)
            {
                var positions = line.positions;
                var newPositions = positions.Select(pos => new Vector2(pos.x, pos.y + difference.y)).ToArray();
                line.SetPositions(newPositions);
            }
        }
    }
}
using Default;
using TMPro;
using UnityEngine;

namespace Line
{
    [RequireComponent(typeof(UGUILineRenderer))]
    public class LineInfo : MonoBehaviour,ILineBeAble
    {
        [SerializeField] private TMP_Text inMaterialText;
        [SerializeField] private TMP_Text outMaterialText;
        public void Init(Vector2 startPoint, Vector2 endPoint, LineType lineType, MaterialType[] materialTypes,int id)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            LineType = lineType;
            MaterialTypes = materialTypes;
            if (LineType == LineType.Boundary)
            {
                inMaterialText.text = MaterialTypes[0].ToString();
                outMaterialText.text = MaterialTypes[1].ToString();
            }
            Id = id;
            GetUGUILineRenderer();
        }

        public void SetMaterialText(float rotation, Vector2 inPos, Vector2 outPos)
        {
            inMaterialText.rectTransform.localRotation = Quaternion.Euler(0,0,rotation);
            outMaterialText.rectTransform.localRotation = Quaternion.Euler(0,0,rotation);
            inMaterialText.rectTransform.localPosition = inPos;
            outMaterialText.rectTransform.localPosition = outPos;
            inMaterialText.gameObject.SetActive(true);
            outMaterialText.gameObject.SetActive(true);
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public UGUILineRenderer GetUGUILineRenderer()
        {
            if(lineRenderer == null) lineRenderer = GetComponent<UGUILineRenderer>();
            return lineRenderer;
        }

        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; private set; }
        public LineType LineType { get; private set; }
        public int Id { get; private set; }
        public MaterialType[] MaterialTypes { get; private set; }
        private UGUILineRenderer lineRenderer;
    }
}
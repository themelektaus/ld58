using UnityEngine;

namespace NodeEditor
{
    using DataRenderer2D;

    public class UILineBounds : MonoBehaviour
    {
        RectTransform rectTransform;
        UILine uiLine;

        void Awake()
        {
            rectTransform = transform as RectTransform;
            uiLine = GetComponentInParent<UILine>();
        }

        void Update()
        {
            var first = GetPoint(uiLine.line.GetFirstPoint().position);
            var last = GetPoint(uiLine.line.GetLastPoint().position);
            Utils.SetRect(rectTransform, first, last);
        }

        Vector3 GetPoint(Vector3 position)
        {
            return uiLine.transform.TransformPoint(position);
        }
    }
}
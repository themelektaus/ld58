using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class PointContainer
    {
        SerializedProperty positionProperty;
        public Vector2 position
        {
            get => positionProperty.vector2Value;
            set => positionProperty.vector2Value = value;
        }

        SerializedProperty tangentProperty;
        public Vector4 tangent
        {
            get => tangentProperty.vector4Value;
            set => tangentProperty.vector4Value = value;
        }

        public int index { get; private set; }

        PositionElement positionElement;
        InTangentElement inTangentElement;
        OutTangentElement outTangentElement;

        public PointContainer(SerializedProperty pointProperty, int index)
        {
            positionElement = new(this);
            positionProperty = pointProperty.FindPropertyRelative("position");
            tangentProperty = pointProperty.FindPropertyRelative("tangent");
            this.index = index;
        }

        public void SetPositionEvent(EventCallback<MouseDownEvent> mouseDownCallback)
        {
            positionElement.RegisterCallback(mouseDownCallback);
        }

        public void AddInTangent(EventCallback<MouseDownEvent> mouseDownCallback)
        {
            inTangentElement = new InTangentElement(this);
            inTangentElement.RegisterCallback(mouseDownCallback);
        }

        public void AddOutTangent(EventCallback<MouseDownEvent> mouseDownCallback)
        {
            outTangentElement = new OutTangentElement(this);
            outTangentElement.RegisterCallback(mouseDownCallback);
        }

        public void AddTo(VisualElement element)
        {
            element.Add(positionElement);

            if (inTangentElement is not null)
                element.Add(inTangentElement);

            if (outTangentElement is not null)
                element.Add(outTangentElement);
        }

        public Vector4 Update(PointContainer prev, PointContainer next)
        {
            var tangentDelta = Vector4.zero;

            if (prev is not null)
            {
                var delta = position - prev.position;
                tangentDelta.x = delta.x;
                tangentDelta.y = delta.y;
            }

            if (next is not null)
            {
                var delta = next.position - position;
                tangentDelta.z = delta.x;
                tangentDelta.w = delta.y;
            }

            return tangentDelta;
        }

        public void UpdateStyle(IAreaProvider areaProvider, PointContainer prev, PointContainer next)
        {
            Vector2 stylePosition;

            stylePosition = areaProvider.GetAbsolute(position);
            positionElement.style.left = stylePosition.x;
            positionElement.style.bottom = stylePosition.y;

            if (prev is not null)
            {
                var delta = position - prev.position;
                stylePosition = areaProvider.GetAbsolute(position + delta * new Vector2(tangent.x, tangent.y));
                inTangentElement.style.left = stylePosition.x;
                inTangentElement.style.bottom = stylePosition.y;
            }

            if (next is not null)
            {
                var delta = next.position - position;
                stylePosition = areaProvider.GetAbsolute(position + delta * new Vector2(tangent.z, tangent.w));
                outTangentElement.style.left = stylePosition.x;
                outTangentElement.style.bottom = stylePosition.y;
            }
        }
    }
}

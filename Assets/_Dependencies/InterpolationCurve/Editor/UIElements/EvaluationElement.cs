using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class EvaluationElement : VisualElement
    {
        public Vector2 position;

        readonly Label label;

        public EvaluationElement()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("point");
            AddToClassList("evaluation");
            label = new() { pickingMode = PickingMode.Ignore };
            Add(label);
        }

        public void Update(IAreaProvider areaProvider, float min, float max)
        {
            var stylePosition = areaProvider.GetAbsolute(position);
            style.left = stylePosition.x;
            style.bottom = stylePosition.y;
            label.text = (min + position.y * (max - min)).ToString("0.00000").Replace(',', '.');
            label.style.marginTop = position.y > .95f ? 30 : 0;
            label.style.marginLeft = position.x > .95f ? -50 : 0;
        }
    }
}

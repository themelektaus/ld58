using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class PositionElement : VisualElement
    {
        public PointContainer pointContainer { get; private set; }

        public PositionElement(PointContainer pointContainer)
        {
            this.pointContainer = pointContainer;
            AddToClassList("point");
        }
    }
}

using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class OutTangentElement : VisualElement
    {
        public PointContainer pointContainer { get; private set; }

        public OutTangentElement(PointContainer pointContainer)
        {
            this.pointContainer = pointContainer;
            AddToClassList("point");
            AddToClassList("out");
        }
    }
}

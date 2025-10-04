using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class InTangentElement : VisualElement
    {
        public PointContainer pointContainer { get; private set; }

        public InTangentElement(PointContainer pointContainer)
        {
            this.pointContainer = pointContainer;
            AddToClassList("point");
            AddToClassList("in");
        }
    }
}

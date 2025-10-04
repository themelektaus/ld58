using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class EvaluationSlider : Slider
    {
        public EvaluationSlider() : base(0, 1)
        {
            style.width = 100;
            style.position = Position.Absolute;
            style.right = 4;
            style.top = 7;
        }
    }
}

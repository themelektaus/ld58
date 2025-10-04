using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class TimelineBarEvent : VisualElement
    {
        public InterpolationCurve.Timeline.Event @event { get; private set; }

        public TimelineBarEvent(InterpolationCurve.Timeline.Event @event)
        {
            this.@event = @event;
        }
    }
}

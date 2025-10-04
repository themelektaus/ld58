using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class TimelineBar : VisualElement
    {
        SerializedObject interpolationCurveObject;

        readonly IAreaProvider areaProvider;
        readonly List<TimelineBarEvent> eventElements = new List<TimelineBarEvent>();

        TimelineBarEvent draggingEvent;

        public TimelineBarEventPropertiesElement propertiesElement { get; private set; }

        Vector2 mousePosition;
        bool ctrl;
        bool ignoreNextMouseUp;

        public TimelineBar(IAreaProvider areaProvider)
        {
            this.areaProvider = areaProvider;

            propertiesElement = new TimelineBarEventPropertiesElement();

            RegisterCallback<MouseMoveEvent>(e =>
            {
                mousePosition = e.localMousePosition;
                ctrl = e.ctrlKey;
            });

            RegisterCallback<MouseUpEvent>(e =>
            {
                if (ignoreNextMouseUp)
                {
                    ignoreNextMouseUp = false;
                    return;
                }

                var interpolationCurve = interpolationCurveObject.targetObject as InterpolationCurve;

                if (draggingEvent is not null)
                {
                    draggingEvent = null;
                    return;
                }

                if (e.button == 1)
                {
                    interpolationCurve.timeline.events.Add(new() { time = areaProvider.GetRelative(mousePosition).x });
                    interpolationCurveObject.Update();
                    Refresh();
                    return;
                }
            });
        }

        public void Refresh()
        {
            Refresh(interpolationCurveObject);
        }

        public void Refresh(SerializedObject interpolationCurveObject)
        {
            Clear();
            this.interpolationCurveObject = interpolationCurveObject;
            if (interpolationCurveObject is null)
                return;

            var interpolationCurve = interpolationCurveObject.targetObject as InterpolationCurve;
            foreach (var @event in interpolationCurve.timeline.events)
            {
                var eventElement = new TimelineBarEvent(@event);
                eventElement.RegisterCallback<MouseDownEvent>(e =>
                {
                    propertiesElement.eventElement = null;
                    if (e.button == 1)
                    {
                        interpolationCurve.timeline.events.Remove(eventElement.@event);
                        interpolationCurveObject.Update();
                        ignoreNextMouseUp = true;
                        Refresh();
                        return;
                    }
                    draggingEvent = eventElement;
                });
                eventElement.RegisterCallback<MouseMoveEvent>(e =>
                {
                    propertiesElement.eventElement = eventElement;
                });
                eventElements.Add(eventElement);
                Add(eventElement);
            }
            Add(propertiesElement);
        }

        public void Update()
        {
            if (interpolationCurveObject is null)
                return;

            if (draggingEvent is not null)
            {
                var time = areaProvider.GetRelative(mousePosition).x;
                if (ctrl)
                    time = Mathf.Round(time * 100) / 100;
                draggingEvent.@event.time = time;
            }

            foreach (var eventElement in eventElements)
            {
                var position = areaProvider.GetAbsolute(new Vector2(eventElement.@event.time, 0));
                eventElement.style.left = position.x;
            }

            propertiesElement?.Update();
        }
    }
}

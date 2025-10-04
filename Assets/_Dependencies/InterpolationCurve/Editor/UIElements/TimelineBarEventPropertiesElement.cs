using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class TimelineBarEventPropertiesElement : VisualElement
    {
        TimelineBarEvent _eventElement;

        readonly TextField nameField;
        readonly FloatField timeField;

        public TimelineBarEventPropertiesElement()
        {
            style.display = DisplayStyle.None;

            nameField = new TextField("Name");
            nameField.Q<Label>().style.width = 60;
            nameField.Q<Label>().style.minWidth = 60;
            nameField.RegisterValueChangedCallback(e =>
            {
                if (_eventElement.@event is not null)
                    _eventElement.@event.name = e.newValue;
            });
            Add(nameField);

            timeField = new FloatField("Time");
            timeField.Q<Label>().style.width = 60;
            timeField.Q<Label>().style.minWidth = 60;
            timeField.RegisterValueChangedCallback(e =>
            {
                if (_eventElement.@event is not null)
                    _eventElement.@event.time = e.newValue;
            });
            Add(timeField);
        }

        public TimelineBarEvent eventElement
        {
            get => _eventElement;
            set
            {
                _eventElement = value;

                if (_eventElement is null)
                {
                    style.display = DisplayStyle.None;
                    return;
                }

                style.display = DisplayStyle.Flex;
                nameField.value = eventElement.@event.name;
                timeField.value = eventElement.@event.time;
            }
        }

        public void Update()
        {
            if (style.display == DisplayStyle.None)
                return;

            style.bottom = _eventElement.style.top;
            style.left = _eventElement.style.left;
        }
    }
}

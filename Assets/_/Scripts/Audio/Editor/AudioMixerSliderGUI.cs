using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    public class AudioMixerSliderGUI : IMGUIContainer
    {
        public Object @object;
        public string propertyPath;
        public Vector2 valueRange = new(0, 1);
        public float defaultValue;

        bool dragging;

        public AudioMixerSliderGUI() : base()
        {
            onGUIHandler = OnGUI;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        SerializedProperty property
            => @object && !propertyPath.IsNullOrEmpty()
                ? new SerializedObject(@object).FindProperty(propertyPath)
                : null;

        public float value
        {
            get
            {
                return property?.floatValue ?? 0;
            }
            set
            {
                var property = this.property;
                if (property is null)
                    return;

                var newValue = Mathf.Clamp(value, valueRange.x, valueRange.y);
                if (property.floatValue == newValue)
                    return;
                
                property.floatValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                MarkDirtyRepaint();
            }
        }

        Rect r => contentRect;
        float s => r.width / 12;
        float h => r.height / 16;
        Rect r_inner => new(0, h / 2 + 8, r.width - s * 2, r.height - h - 18);
        Rect r_slide => new(r.width / 2 - s / 2, r_inner.y, s, r_inner.height);

        Rect r_button
        {
            get
            {
                var t = Mathf.InverseLerp(valueRange.x, valueRange.y, value);

                return new(
                    (r.width - r_inner.width) / 2,
                    r_inner.y + r_inner.height * (1 - t) - h / 2,
                    r_inner.width,
                    h
                );
            }
        }

        void OnGUI()
        {
            EditorGUI.DrawRect(r, new(0, 0, 0, .3f));
            EditorGUI.DrawRect(r_slide, new(0, 0, 0, .4f));
            EditorGUI.DrawRect(r_button, new(1, 1, .6f, dragging ? .8f : .4f));
        }

        void OnMouseDown(MouseDownEvent e)
        {
            switch (e.button)
            {
                case 0:
                    dragging = true;
                    break;

                case 1:
                    value = defaultValue;
                    break;
            }
        }

        void OnMouseMove(MouseMoveEvent e)
        {
            if (!dragging)
                return;

            var t = 1 - (e.localMousePosition.y - r_inner.y) / r_inner.height;
            value = Mathf.Lerp(valueRange.x, valueRange.y, t);
        }

        void OnMouseUp(MouseUpEvent e)
        {
            switch (e.button)
            {
                case 0:
                    dragging = false;
                    break;
            }
        }
    }
}

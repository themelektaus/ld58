using System;

using UnityEditor;

using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomEditor(typeof(Notes))]
    public class NotesEditor : UnityEditor.Editor
    {
        static DateTime lastClick = DateTime.MinValue;

        public override VisualElement CreateInspectorGUI()
        {
            var root = this.CreateRootElement(new(1, 1, 0, .1f));
            root.AddVisualTreeAsset<NotesEditor>();

            var textProperty = serializedObject.FindProperty("text");

            var label = root.Q<Label>();

            var textField = root.Q<TextField>();
            textField.style.display = DisplayStyle.None;

            label.text = textProperty.stringValue;
            label.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button != 0)
                    return;

                var now = DateTime.Now;
                if ((now - lastClick).TotalSeconds > .5)
                {
                    lastClick = now;
                    return;
                }

                lastClick = DateTime.MinValue;

                label.style.display = DisplayStyle.None;

                textField.value = textProperty.stringValue;
                textField.style.display = DisplayStyle.Flex;
                textField.RegisterValueChangedCallback(e =>
                {
                    textProperty.stringValue = e.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
            });

            return root;
        }
    }
}

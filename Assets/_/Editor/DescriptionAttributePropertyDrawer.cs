using UnityEditor;

using UnityEngine;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(DescriptionAttribute))]
    public class DescriptionAttributePropertyDrawer : PropertyDrawer
    {
        static readonly float spacing = EditorGUIUtility.standardVerticalSpacing;

        DescriptionAttribute description => attribute as DescriptionAttribute;
        float height => description.height * 12 + 6;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + height + spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = height;
            EditorGUI.HelpBox(position, description.text, (UnityEditor.MessageType) (int) description.type);

            position.y += height + spacing;
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}

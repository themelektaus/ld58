using UnityEditor;

using UnityEngine;

using F = System.Reflection.BindingFlags;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(AppearanceAttribute))]
    [CustomPropertyDrawer(typeof(HiddenAttribute))]
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    [CustomPropertyDrawer(typeof(ReadOnlyInPlayModeAttribute))]
    [CustomPropertyDrawer(typeof(ReadOnlyInEditModeAttribute))]
    public class AppearanceAttributePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
                return 0;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
                return;

            if (IsEditable(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var enabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = enabled;
        }

        bool ShouldApply()
        {
            var x = attribute as AppearanceAttribute;

            if (Application.isEditor && x.editorFlags.HasFlag(EditorFlags.EditMode))
                return true;

            if (Application.isPlaying && x.editorFlags.HasFlag(EditorFlags.PlayMode))
                return true;

            return false;
        }

        bool IsVisible(SerializedProperty property)
        {
            if (!ShouldApply())
                return true;

            var x = attribute as AppearanceAttribute;
            if (x.displayFlags.HasFlag(DisplayFlags.Visible))
                return true;

            if (Validate(property, "Hidden"))
                return false;

            return true;
        }

        bool IsEditable(SerializedProperty property)
        {
            if (!ShouldApply())
                return true;

            var x = attribute as AppearanceAttribute;
            if (x.displayFlags.HasFlag(DisplayFlags.Editable))
                return true;

            if (Validate(property, "ReadOnly"))
                return false;

            return true;
        }

        static bool Validate(SerializedProperty property, string name)
        {
            var target = property.serializedObject.targetObject;

            var method = target.GetType().GetMethod(
                $"__{property.propertyPath}__{name}",
                F.Instance | F.Public | F.NonPublic
            );

            if (method is null)
                return true;

            if ((bool) method.Invoke(target, null))
                return true;

            return false;
        }
    }
}

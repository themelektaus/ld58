using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributePropertyDrawer : PropertyDrawer
    {
        const BindingFlags METHOD_FLAGS
            = BindingFlags.Instance
            | BindingFlags.DeclaredOnly
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        MethodInfo[] methods;
        MethodInfo method;

        const float MARGIN_TOP = 2;
        const float MARGIN_BOTTOM = 1;
        const float WIDTH = 200;
        const float HEIGHT = 21;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return HEIGHT + MARGIN_TOP + MARGIN_BOTTOM;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
                return;

            var width = position.width;
            var buttonWidth = Mathf.Min(WIDTH, width);

            position.height = HEIGHT;
            position.width = buttonWidth;
            position.y += MARGIN_TOP;
            position.x = (width - buttonWidth) / 2 + 10;

            var attr = attribute as ButtonAttribute;
            var propertyName = property.name.Trim('_');

            if (!GUI.Button(position, attr.text ?? propertyName.Replace('_', ' ')))
                return;

            var target = property.serializedObject.targetObject;

            methods ??= target.GetType().GetMethods(METHOD_FLAGS);

            if (method is null)
            {
                var methodName = (attr.method ?? propertyName).ToLower();
                method = methods.FirstOrDefault(x => x.Name.ToLower().Equals(methodName));
            }

            method.Invoke(target, null);
        }
    }
}

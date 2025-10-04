using UnityEditor;

using UnityEngine;

namespace InterpolationCurve.Editor
{
    [CustomPropertyDrawer(typeof(InterpolationCurve))]
    public class InterpolationCurvePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 33;
        }

        static GUIStyle _objectSelectorButtonStyle;
        static GUIStyle objectSelectorButtonStyle
        {
            get
            {
                if (_objectSelectorButtonStyle is null)
                    _objectSelectorButtonStyle = new(GUI.skin.button)
                    {
                        padding = new RectOffset(),
                        fontSize = 24
                    };
                return _objectSelectorButtonStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var width = position.width;

            position.width = EditorGUIUtility.labelWidth;
            position.height -= 3;
            GUI.Label(position, label, new(GUI.skin.label) { alignment = TextAnchor.UpperLeft });

            position.x += position.width + 2;
            position.y += 2;
            position.height -= 4;
            position.width = width - EditorGUIUtility.labelWidth - 12;

            var interpolationCurve = Utils.GetObject<InterpolationCurve>(property);
            Utils.DrawInterpolationCurve(position, interpolationCurve, false);

            if (Utils.IsMouseDown(position))
                UIElements.InterpolationCurveWindow.Open(interpolationCurve);

            position.x += position.width;
            position.width = 11;

            if (GUI.Button(position, GUIContent.none, objectSelectorButtonStyle))
                UIElements.InterpolationCurvesWindow.Create(property);
        }
    }
}

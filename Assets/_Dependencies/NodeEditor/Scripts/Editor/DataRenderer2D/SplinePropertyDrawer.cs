using UnityEditor;
using UnityEngine;

namespace NodeEditor.DataRenderer2D.Editors
{
    [CustomPropertyDrawer(typeof(Spline))]
    public class SplinePropertyDrawer : PropertyDrawer
    {
        float PointHeight => 4.7f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 1f;
            var mode = property.FindPropertyRelative("mode").enumValueIndex;

            if (mode == 0)
                height += property.FindPropertyRelative("points").isExpanded ? property.FindPropertyRelative("points").arraySize * PointHeight + 2f : 1f;
            else if (mode == 1)
                height += property.FindPropertyRelative("pair").isExpanded ? 2f * PointHeight + 2f : 1f;

            height += property.FindPropertyRelative("option").isExpanded ? 10f : 1f; ;

            return height * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("mode"));
            position.y += EditorGUIUtility.singleLineHeight;

            var mode = property.FindPropertyRelative("mode").enumValueIndex;

            float height;

            if (mode == 0)
            {
                height = property.FindPropertyRelative("points").isExpanded ? (property.FindPropertyRelative("points").arraySize * PointHeight + 2f) : 1f;
                height *= EditorGUIUtility.singleLineHeight;
                position.height = height;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("points"), true);
                position.y += height;
            }
            else if (mode == 1)
            {
                height = property.FindPropertyRelative("pair").isExpanded ? 2f * PointHeight + 2f : 1f;
                height *= EditorGUIUtility.singleLineHeight;

                position.height = height;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("pair"), true);
                position.y += height;
            }

            height = property.FindPropertyRelative("option").isExpanded ? 10f : 1f;
            height *= EditorGUIUtility.singleLineHeight;
            position.height = height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("option"), true);

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
using UnityEditor;

using UnityEngine;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(ReferenceCollection))]
    public class ReferenceCollectionPropertyDrawer : PropertyDrawer
    {
        static float lineHeight => EditorGUIUtility.singleLineHeight;
        static float verticalSpace => EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 2;
            var items = property.FindPropertyRelative("items");
            height += items.arraySize * (ReferencePropertyDrawer.GetHeight() + verticalSpace);
            height += lineHeight + 8;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;
            float x = position.x;
            float width = position.width;
            var items = property.FindPropertyRelative("items");
            for (int i = 0; i < items.arraySize; i++)
            {
                position.x = x;
                position.width = width - 23;
                position.height = ReferencePropertyDrawer.GetHeight();
                EditorGUI.PropertyField(position, items.GetArrayElementAtIndex(i));
                position.x = x + width - 21;
                position.width = 21;
                position.height = 21;
                if (GUI.Button(position, "X"))
                {
                    items.DeleteArrayElementAtIndex(i);
                }
                position.y += ReferencePropertyDrawer.GetHeight() + verticalSpace;
            }
            position.x = x;
            position.width = width;
            position.height = lineHeight + 2;
            if (GUI.Button(position, "Add Reference"))
            {
                items.InsertArrayElementAtIndex(items.arraySize);
                position.y += position.height + verticalSpace;
            }
        }
    }
}

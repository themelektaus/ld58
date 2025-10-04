using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(Reference))]
    public class ReferencePropertyDrawer : PropertyDrawer
    {
        public static float GetHeight()
        {
            return 89 + (Application.isPlaying ? 20 : 0);
        }

        public UnityEditor.UIElements.ObjectField objectField = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative("type");
            var objectProperty = property.FindPropertyRelative("object");

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 2, 0),
                padding = new RectOffset(0, 0, 1, 2)
            };

            if (label.text.IsNullOrEmpty())
            {
                EditorGUI.BeginChangeCheck();

                var _labelWidth = 150;
                var p = position;
                p.width = _labelWidth;
                EditorGUI.PropertyField(p, typeProperty, GUIContent.none);

                p = position;
                p.x += _labelWidth + 2;
                p.width -= _labelWidth + 30;
                EditorGUI.PropertyField(p, property.FindPropertyRelative("path"), GUIContent.none);

                if (objectField != null && objectField.value)
                {
                    p.x += p.width + 3;
                    p.width = 23;
                    if (GUI.Button(p, "...", buttonStyle))
                    {
                        var menu = new GenericMenu();
                        var fieldTypes = System.Enum.GetValues(typeof(Reference.Type));
                        var fieldType = (Reference.Type) fieldTypes.GetValue(typeProperty.enumValueIndex);
                        var @object = objectField.value;
                        AddMenuItems(menu, @object, property, fieldType);
                        menu.ShowAsContext();
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
                return;
            }

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth /= 1.8f;

            Offset(ref position, 0, 2);
            EditorGUI.DrawRect(position, new Color(.4f, 1, 1, .3f));

            Offset(ref position, 1, 1);
            EditorGUI.DrawRect(position, new Color(0, 0, 0, .3f));

            Height(ref position, 18);
            EditorGUI.DrawRect(position, new Color(0, 0, 0, .3f));

            Offset(ref position, 2, 1, -4, 0);
            EditorGUI.LabelField(position, label.text, EditorStyles.boldLabel);

            var tempPosition = position;
            tempPosition.x += tempPosition.width - EditorGUIUtility.labelWidth - 2;
            tempPosition.width = EditorGUIUtility.labelWidth;
            var tempStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleRight
            };
            tempStyle.normal.textColor = new Color(.4f, 1, 1, .6f);
            EditorGUI.LabelField(tempPosition, nameof(Reference), tempStyle);

            EditorGUI.BeginChangeCheck();

            Offset(ref position, 3, 20, -6, 0);
            EditorGUI.PropertyField(position, typeProperty);

            float x = position.x;
            float width = position.width;

            Offset(ref position, 0, 20, 0, 0);
            EditorGUI.PropertyField(position, objectProperty);

            Offset(ref position, 0, 20, -25, 0);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("path"));

            Offset(ref position, position.width + 2, 0, 0, 0);
            Width(ref position, 23);
            if (GUI.Button(position, "...", buttonStyle))
            {
                if (objectProperty.objectReferenceValue)
                {
                    var menu = new GenericMenu();
                    var fieldTypes = System.Enum.GetValues(typeof(Reference.Type));
                    var fieldType = (Reference.Type) fieldTypes.GetValue(typeProperty.enumValueIndex);
                    var @object = objectProperty.objectReferenceValue;
                    AddMenuItems(menu, @object, property, fieldType);
                    menu.ShowAsContext();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            if (Application.isPlaying)
            {
                position.x = x;
                position.y += 20;
                position.width = width;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, "Saved Data", property.FindPropertyRelative("serializedData").stringValue);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        void Offset(ref Rect position, float x, float y)
        {
            position.x += x;
            position.y += y;
            position.width -= x * 2;
            position.height -= y * 2;
        }

        void Offset(ref Rect position, float x, float y, float width, float height)
        {
            position.x += x;
            position.y += y;
            position.width += width;
            position.height += height;
        }

        void Width(ref Rect position, float width)
        {
            position.width = width;
        }

        void Height(ref Rect position, float height)
        {
            position.height = height;
        }

        static void AddMenuItems(GenericMenu menu, Object root, SerializedProperty property, Reference.Type fieldType)
        {
            string name;

            if (root is UnityEngine.ScriptableObject)
            {
                name = root.name;
            }
            else
            {
                name = nameof(GameObject);
                root = root.GetGameObject();
            }

            var rootItems = new List<string> { name };
            AddMenuItems(menu, root, name, property, fieldType, root, new());
            foreach (var component in root.GetComponents<Component>())
            {
                string rootItem;
                var index = 0;
                do
                {
                    index++;
                    rootItem = component.GetType().Name.Replace("/", "");
                    if (index > 1)
                        rootItem += $" ({index})";
                } while (rootItems.Contains(rootItem));
                rootItems.Add(rootItem);

                AddMenuItems(menu, component, rootItem, property, fieldType, component, new());
            }
        }

        static void AddMenuItems(GenericMenu menu, Object root, string rootItem, SerializedProperty property, Reference.Type fieldType, object @object, List<string> tree)
        {
            var infos = GetInfos(fieldType, @object);

            foreach (var (name, _) in infos.OrderBy(x => x.name))
            {
                var content = new GUIContent(name);
                if (tree.Count > 0)
                    content.text = string.Join("/", tree) + "/" + content.text;
                content.text = $"{rootItem}/{content.text}";

                menu.AddItem(content, false, OnContextMenu, new object[] {
                    property,
                    root,
                    (tree.Count == 0 ? "" : $"{string.Join(".", tree)}.") + name
                });
            }

            foreach (var (name, type) in infos.OrderBy(x => x.name))
            {
                if (tree.Count < 2)
                {
                    if (!type.IsPrimitive && !type.IsEnum && type != typeof(string))
                    {
                        tree.Add(name);
                        AddMenuItems(menu, root, rootItem, property, fieldType, type, tree);
                        tree.Remove(name);
                    }
                }
            }
        }

        static List<(string name, System.Type type)> GetInfos(Reference.Type fieldType, object @object)
        {
            var flags = Reference.FLAGS;
            var infos = new List<(string name, System.Type type)>();
            var type = @object is System.Type t ? t : @object.GetType();

            switch (fieldType)
            {
                case Reference.Type.Member:
                    infos.AddRange(type.GetFields(flags).Select(x => (x.Name, x.FieldType)));
                    infos.AddRange(type.GetProperties(flags)
                        .Where(x => x.Name != "Item" && x.Name != "Items")
                        .Where(x => x.Name == "enabled" || x.DeclaringType == type)
                        .Select(x => (x.Name, x.PropertyType)));
                    break;

                case Reference.Type.AnimatorParameter:
                    if (@object is Animator animator)
                        infos.AddRange(animator.GetParameterNames().Select(x => (x, typeof(string))));
                    break;
            }

            return infos;
        }

        static void OnContextMenu(object data)
        {
            var dataArray = data as object[];
            var fieldProperty = dataArray[0] as SerializedProperty;
            var @object = dataArray[1] as Object;
            var path = dataArray[2] as string;
            fieldProperty.FindPropertyRelative("object").objectReferenceValue = @object;
            fieldProperty.FindPropertyRelative("path").stringValue = path;
            fieldProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}

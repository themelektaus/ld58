using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace QuickMenu
{
    public class QuickMenuExecutionsWindow : EditorWindow
    {
        [UnityEditor.MenuItem("Assets/Quick Menu Executions")]
        public static void Open()
        {
            var window = CreateWindow<QuickMenuExecutionsWindow>();
            window.titleContent = new GUIContent("Quick Menu Executions");
        }

        [SerializeField]
        VisualTreeAsset visualTreeAsset_MenuItem;

        VisualElement root => rootVisualElement;
        ScrollView content;

        HashSet<QuickMenuExecution> executionAssets;

        bool refreshing;

        public void CreateGUI()
        {
            if (!visualTreeAsset_MenuItem)
                return;

            executionAssets = AssetDatabase.FindAssets($"t:{typeof(QuickMenuExecution).FullName}")
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<QuickMenuExecution>(x))
                .ToHashSet();

            content = new ScrollView();
            root.Add(content);

            content.StretchToParentSize();

            Refresh();
        }

        void Refresh()
        {
            content.Clear();

            foreach (var executionAsset in executionAssets)
            {
                var foldout = new Foldout
                {
                    text = executionAsset.name,
                    value = false,
                    userData = new SerializedObject(executionAsset)
                };

                Refresh(foldout);

                content.Add(foldout);
            }
        }

        void Refresh(Foldout foldout)
        {
            foldout.Clear();

            var executionAssetObject = foldout.userData as SerializedObject;

            var menuItemObjects = executionAssetObject.FindProperty("menuItems");

            var arraySize = menuItemObjects.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                var menuItemObject = menuItemObjects.GetArrayElementAtIndex(i);

                var menuItemElement = foldout.AddVisualTreeAsset(visualTreeAsset_MenuItem, stretch: false);

                var menuItemElementContent = menuItemElement.Q("Content");

                menuItemElementContent.Insert(0, CreateAddButton(foldout, menuItemObjects, i));
                menuItemElementContent.Add(CreateDeleteButton(foldout, menuItemObjects, i));
                menuItemElementContent.Add(CreateAddButton(foldout, menuItemObjects, i + 1));

                menuItemElement.BindProperty(menuItemObject);
            }
        }

        Button CreateAddButton(Foldout foldout, SerializedProperty menuItemObjects, int index)
        {
            return new(() =>
            {
                menuItemObjects.InsertArrayElementAtIndex(index);

                var property = menuItemObjects.GetArrayElementAtIndex(index);

                // Set default values
                //property.FindPropertyRelative("visible").boolValue = true;
                //property.FindPropertyRelative("category").stringValue = "";
                //property.FindPropertyRelative("subCategory").stringValue = "";
                //property.FindPropertyRelative("title").stringValue = "";
                //property.FindPropertyRelative("description").stringValue = "";
                //property.FindPropertyRelative("path").stringValue = "";

                menuItemObjects.serializedObject.ApplyModifiedProperties();

                Refresh(foldout);
            })
            {
                text = "+"
            };
        }

        Button CreateDeleteButton(Foldout foldout, SerializedProperty menuItemObjects, int index)
        {
            return new(() =>
            {
                menuItemObjects.DeleteArrayElementAtIndex(index);
                menuItemObjects.serializedObject.ApplyModifiedProperties();

                Refresh(foldout);
            })
            {
                text = "X"
            };
        }
    }
}
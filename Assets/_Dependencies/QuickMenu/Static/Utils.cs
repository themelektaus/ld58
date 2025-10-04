using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using Flags = System.Reflection.BindingFlags;

namespace QuickMenu
{
    public static class Utils
    {
        static Dictionary<string, QuickMenuCategoryDefinition.Category> _categories;
        static Dictionary<string, QuickMenuCategoryDefinition.Category> categories
        {
            get
            {
                _categories ??= new();

                if (_categories.Count == 0)
                {
                    _categories = AssetDatabase.FindAssets($"t:{typeof(QuickMenuCategoryDefinition).FullName}")
                        .Select(x => AssetDatabase.GUIDToAssetPath(x))
                        .Select(x => AssetDatabase.LoadAssetAtPath<QuickMenuCategoryDefinition>(x))
                        .SelectMany(x => x.categories)
                        .ToDictionary(x => x.name.ToLower());
                }

                return _categories;
            }
        }

        public static QuickMenuCategoryDefinition.Category GetCategory(string name)
        {
            name = (name ?? "").ToLower();

            if (categories.ContainsKey(name))
                return categories[name];

            return new() { name = name, color = new(1, 1, 1, .5f) };
        }

        public static readonly Type projectBrowser = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
        public static readonly Type sceneHierarchy = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchy");
        public static readonly Type sceneHierarchyWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        public static readonly Type animatorControllerTool = typeof(UnityEditor.Graphs.Graph).Assembly.GetType("UnityEditor.Graphs.AnimatorControllerTool");

        // Source: http://answers.unity.com/answers/1817811/view.html
        public static bool TryGetActiveFolderPath(out string path)
        {
            var type = typeof(ProjectWindowUtil);
            var method = type.GetMethod("TryGetActiveFolderPath", Flags.Static | Flags.NonPublic);
            var args = new object[] { null };
            bool found = (bool) method.Invoke(null, args);
            path = (string) args[0];
            return found;
        }
    }
}

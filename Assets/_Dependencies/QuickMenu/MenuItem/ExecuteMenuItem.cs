using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine.UIElements;

namespace QuickMenu
{
    internal class ExecuteMenuItem : IMenuItem
    {
        [AddQuickMenuItems]
        public static IEnumerable<IMenuItem> _AddQuickMenuItems()
        {
            foreach (var executionAsset in executionAssets)
                foreach (var menuItem in executionAsset.menuItems)
                    yield return new ExecuteMenuItem(executionAsset, menuItem);
        }

        static HashSet<QuickMenuExecution> _executionAssets;
        static HashSet<QuickMenuExecution> executionAssets
        {
            get
            {
                if (_executionAssets is null || _executionAssets.Count == 0)
                    _executionAssets = AssetDatabase.FindAssets($"t:{typeof(QuickMenuExecution).FullName}")
                        .Select(x => AssetDatabase.GUIDToAssetPath(x))
                        .Select(x => AssetDatabase.LoadAssetAtPath<QuickMenuExecution>(x))
                        .Where(x => x.active)
                        .ToHashSet();

                return _executionAssets;
            }
        }

        public bool visible
        {
            get
            {
                if (!execution.visible)
                    return false;

                if (!menuItem.visible)
                    return false;

                return true;
            }
        }

        public int priority => 0;

        public string title
        {
            get
            {
                if (!string.IsNullOrEmpty(menuItem.title))
                    return menuItem.title;

                var title = menuItem.path;

                if (title.StartsWith("Assets/"))
                    title = title[7..];

                else if (title.StartsWith("GameObject/"))
                    title = title[11..];

                var categoryName = menuItem.category.ToString();
                if (title.StartsWith($"{categoryName}/", StringComparison.InvariantCultureIgnoreCase))
                    title = title[(categoryName.Length + 1)..];

                return title.Trim('/').Replace('_', ' ').SplitCamelCase().Replace("/", " \u2192 ");
            }
        }

        public string description => menuItem.description;

        public string category => menuItem.category;
        public string subCategory => menuItem.subCategory;

        readonly QuickMenuExecution execution;
        readonly QuickMenuExecution.MenuItem menuItem;

        public ExecuteMenuItem(
            QuickMenuExecution execution,
            QuickMenuExecution.MenuItem menuItem
        )
        {
            this.execution = execution;
            this.menuItem = menuItem;
        }

        public bool Validation(Context context)
        {
            if (context.isAnimatorControllerTool)
                return false;

            if (menuItem.path.StartsWith("GameObject/"))
            {
                if (!context.isSceneHierarchy)
                    return false;
                
                if (menuItem.requiresTransform && !context.transform)
                    return false;
                
                return true;
            }

            if (menuItem.path.StartsWith("Assets/"))
                return context.isProjectBrowserWithAsset;

            return true;
        }

        public bool Command(Context context)
        {
            var path = menuItem.path;
            
            switch (path)
            {
                case "GameObject/Properties...":
                    path = "Assets/Properties...";
                    break;
            }
            
            if (path.StartsWith("GameObject/"))
                EditorUtility.SetDefaultParentObject(context.gameObject);


            EditorApplication.ExecuteMenuItem(path);
            EditorUtility.ClearDefaultParentObject();
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }
    }
}

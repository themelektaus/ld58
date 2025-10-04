using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuickMenu
{
    internal class CreateAssetMenuItem : IMenuItem
    {
        [AddQuickMenuItems]
        public static IEnumerable<IMenuItem> _AddQuickMenuItems()
        {
            var createAssetMenus = TypeCache
                .GetTypesWithAttribute<CreateAssetMenuAttribute>()
                .Select(x => (x, x.GetCustomAttribute<CreateAssetMenuAttribute>()));

            foreach (var (type, attribute) in createAssetMenus)
                yield return new CreateAssetMenuItem(type, attribute);
        }

        public bool visible => true;
        public int priority => 0;

        public string title =>
            attribute.menuName?.Replace("/", " \u2192 ") ??
            type.Name.SplitCamelCase().Replace('_', ' ');

        public string description => null;

        public string category => "Asset";
        public string subCategory => null;

        readonly Type type;
        readonly CreateAssetMenuAttribute attribute;

        public CreateAssetMenuItem(Type type, CreateAssetMenuAttribute attribute)
        {
            this.type = type;
            this.attribute = attribute;
        }

        public bool Validation(Context context)
        {
            return context.isProjectBrowserWithAsset;
        }

        public bool Command(Context context)
        {
            var menuName = attribute.menuName ?? type.Name.SplitCamelCase();
            EditorApplication.ExecuteMenuItem($"Assets/Create/{menuName}");
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }
    }
}
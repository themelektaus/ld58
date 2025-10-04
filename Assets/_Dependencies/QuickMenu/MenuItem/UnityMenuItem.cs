using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace QuickMenu
{
    internal class UnityMenuItem : IMenuItem
    {
        // MyTODO: Generate QuickMenu Executions
        //[AddQuickMenuItems]
        public static IEnumerable<IMenuItem> _AddQuickMenuItems()
        {
            var menus = TypeCache
                .GetMethodsWithAttribute<UnityEditor.MenuItem>()
                .Select(x => (x, x.GetCustomAttribute<UnityEditor.MenuItem>()));

            foreach (var (methodInfo, attribute) in menus)
                yield return new UnityMenuItem(methodInfo, attribute);
        }

        public bool visible => false;
        public int priority => 0;

        public string title =>
            attribute.menuItem?.Replace("/", " \u2192 ") ??
            methodInfo.Name.SplitCamelCase().Replace('_', ' ');

        public string description => null;

        public string category => "Unity";
        public string subCategory => null;

        readonly MethodInfo methodInfo;
        readonly UnityEditor.MenuItem attribute;

        public UnityMenuItem(MethodInfo methodInfo, UnityEditor.MenuItem attribute)
        {
            this.methodInfo = methodInfo;
            this.attribute = attribute;
        }

        public bool Validation(Context context)
        {
            return true;
        }

        public bool Command(Context context)
        {
            var menuName = attribute.menuItem ?? methodInfo.Name.SplitCamelCase();
            EditorApplication.ExecuteMenuItem($"{menuName}");
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }
    }
}
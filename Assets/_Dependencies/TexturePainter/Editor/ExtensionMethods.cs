using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine.UIElements;

namespace TexturePainter.Editor
{
    public static class ExtensionMethods
    {
        public static void LoadNestedElements(this VisualElement root, VisualTreeAsset rootLayout)
        {
            foreach (var container in root.Query<TemplateContainer>().ToList())
            {
				if (container.templateSource == rootLayout)
                    continue;

                var name = container.templateSource.name;
                var asset = _LoadVisualTreeAsset(name);
                if (!asset)
                    continue;

                var assembly = Assembly.GetExecutingAssembly();
                var @namespace = typeof(ExtensionMethods).Namespace;
                var type = assembly.GetType($"{@namespace}.{name}");
                if (type is null)
                    continue;

                var instance = Activator.CreateInstance(type) as VisualElement;
                instance.Add(asset.CloneTree());

                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var createMethod = instance.GetType().GetMethod("OnCreate", flags);
                createMethod?.Invoke(instance, null);

                container.Clear();
                container.Add(instance);
            }
        }

        static Dictionary<string, VisualTreeAsset> visualTreeAssets;

        static VisualTreeAsset _LoadVisualTreeAsset(string name)
        {
            if (visualTreeAssets is null)
            {
                visualTreeAssets = new();
                foreach (var asset in Utils.FindAssets<VisualTreeAsset>())
                    visualTreeAssets[asset.name] = asset;
            }

            if (visualTreeAssets.ContainsKey(name))
                return visualTreeAssets[name];

            return null;
        }
    }
}
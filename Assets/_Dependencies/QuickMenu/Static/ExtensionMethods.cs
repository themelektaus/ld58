using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using Regex = System.Text.RegularExpressions.Regex;

namespace QuickMenu
{
    public static class ExtensionMethods
    {
        public static TemplateContainer AddVisualTreeAsset(this VisualElement @this, VisualTreeAsset visualTreeAsset, bool stretch)
        {
            var templateContainer = visualTreeAsset.Instantiate();
            @this.Add(templateContainer);
            if (stretch)
                templateContainer.StretchToParentSize();
            return templateContainer;
        }

        public static void Show(this VisualElement @this, bool visible)
        {
            @this.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetBorderColor(this IStyle @this, Color color)
        {
            @this.borderTopColor = color;
            @this.borderRightColor = color;
            @this.borderBottomColor = color;
            @this.borderLeftColor = color;
        }

        public static float SearchMatch(this string @this, string keywords)
        {
            var _search = @this.ToLower().Split(' ').Where(x => x.Length > 0).Distinct().ToList();
            var _keywords = keywords.ToLower().Split(' ').Where(x => x.Length > 0).Distinct().ToList();

            foreach (var _keyword in _keywords)
            {
                if (_search.Any(x => x.Contains(_keyword)))
                    continue;

                return 0;
            }

            float result = 1;

            int index;

            foreach (var _keyword in _keywords)
            {
                index = _search.FindIndex(x => x.Equals(_keyword));
                if (index > -1)
                    result += 1 - (float) index / _search.Count;

                index = _search.FindIndex(x => x.StartsWith(_keyword));
                if (index > -1)
                    result += 1 - (float) index / _search.Count;
            }

            return result;
        }

        public static string SplitCamelCase(this string @this)
        {
            if (@this == string.Empty)
                return @this;

            @this = Regex.Replace(@this, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2");
            @this = Regex.Replace(@this, @"(\p{Ll})(\P{Ll})", "$1 $2");

            @this = @this.Replace("_ ", "_");
            @this = @this.Replace(" _", "_");

            @this = @this.Replace("/ ", "/");
            @this = @this.Replace(" /", "/");

            Span<char> chars = stackalloc char[@this.Length];
            @this.AsSpan(1).CopyTo(chars[1..]);
            chars[0] = char.ToUpper(@this[0]);
            @this = new string(chars);

            return string.Join(' ', @this.Split(' ').Where(x => x != string.Empty));
        }

        public static void ForcePreventDefault(this EventBase @this)
        {
            @this.StopPropagation();
			if (@this.target is VisualElement element)
			    element.focusController?.IgnoreEvent(@this);
		}
	}
}

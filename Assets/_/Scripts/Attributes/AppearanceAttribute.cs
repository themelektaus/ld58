using System;
using System.Reflection;

using UnityEngine;

namespace Prototype
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AppearanceAttribute : PropertyAttribute
    {
        public DisplayFlags displayFlags { get; protected set; } = DisplayFlags.Visible | DisplayFlags.Editable;
        public EditorFlags editorFlags { get; protected set; } = EditorFlags.EditMode | EditorFlags.PlayMode;

        public static bool IsVisible(object @object)
        {
            var appearance = @object.GetType().GetCustomAttribute<AppearanceAttribute>();
            if (appearance is null)
                return true;

            if (appearance.displayFlags.HasFlag(DisplayFlags.Visible))
                return true;

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HiddenAttribute : AppearanceAttribute
    {
        public HiddenAttribute()
        {
            displayFlags ^= DisplayFlags.Visible;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : AppearanceAttribute
    {
        public ReadOnlyAttribute()
        {
            displayFlags ^= DisplayFlags.Editable;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyInEditModeAttribute : ReadOnlyAttribute
    {
        public ReadOnlyInEditModeAttribute()
        {
            editorFlags = EditorFlags.EditMode;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyInPlayModeAttribute : ReadOnlyAttribute
    {
        public ReadOnlyInPlayModeAttribute()
        {
            editorFlags = EditorFlags.PlayMode;
        }
    }
}

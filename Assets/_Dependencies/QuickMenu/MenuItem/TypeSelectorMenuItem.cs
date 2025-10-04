using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace QuickMenu
{
    public class TypeSelectorMenuItem : IMenuItem
    {
        public bool visible => true;
        public int priority => 0;

        public string title => type.Name;

        public string description => type.FullName;

        public string category => null;

        public string subCategory => null;

        readonly Type type;
        readonly Action<Type> command;

        public TypeSelectorMenuItem(Type type, Action<Type> command)
        {
            this.type = type;
            this.command = command;
        }

        public bool Command(Context context)
        {
            command(type);
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }

        public bool Validation(Context context)
        {
            return true;
        }
    }
}
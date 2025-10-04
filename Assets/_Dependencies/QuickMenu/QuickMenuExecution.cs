using System;
using System.Collections.Generic;

using UnityEngine;

namespace QuickMenu
{
    [CreateAssetMenu(menuName = "Quick Menu Execution")]
    public class QuickMenuExecution : ScriptableObject
    {
        public bool active = true;
        public bool visible = true;

        [Serializable]
        public struct MenuItem
        {
            public bool visible;
            public string category;
            public string subCategory;
            public string title;
            public string description;
            public string path;
            public bool requiresTransform;
        }

        public List<MenuItem> menuItems;
    }
}

using System.Linq;

using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ObjectByNameSelector : ObjectSelector
    {
        public string name;

        public override bool Match(Object value, bool _)
        {
            if (!value)
                return name.IsNullOrEmpty();

            return value.name == name;
        }

        public override Object Find()
        {
            return Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).FirstOrDefault(x => Match(x, false));
        }

        public override Object[] FindAll()
        {
            return Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).Where(x => Match(x, false)).ToArray();
        }
    }
}

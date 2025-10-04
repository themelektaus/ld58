using System.Linq;

using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ObjectByComponentSelector : ObjectSelector
    {
        [Type] public string type;
        string lastType;

        System.Type _currentType;
        System.Type currentType
        {
            get
            {
                if (lastType != type)
                {
                    lastType = type;
                    _currentType = null;
                }

                if (_currentType is null)
                    _currentType = Utils.types.FirstOrDefault(x => Match(x));

                return _currentType;
            }
        }

        public override bool Match(Object value, bool includeTree)
        {
            if (!value)
                return currentType is null;

            var gameObject = value.GetGameObject();
            if (!gameObject)
                return currentType is null;

            if (gameObject.GetComponent(currentType))
                return true;

            if (!includeTree)
                return false;

            if (gameObject.GetComponentInChildren(currentType, true))
                return true;

            if (gameObject.GetComponentInParent(currentType, true))
                return true;

            return false;
        }

        bool Match(System.Type type)
        {
            return type.FullName == this.type;
        }

        public override Object Find()
        {
            if (currentType is null)
                return null;

            return Object.FindFirstObjectByType(currentType, FindObjectsInactive.Include);
        }

        public override Object[] FindAll()
        {
            if (currentType is null)
                return new Object[0];

            return Object.FindObjectsByType(currentType, FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
    }
}

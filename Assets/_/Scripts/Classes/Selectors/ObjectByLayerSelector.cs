using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ObjectByLayerSelector : ObjectSelector
    {
        public LayerMask layerMask;

        public override bool Match(Object value, bool _)
        {
            if (!value)
                return layerMask == -1;

            var gameObject = value.GetGameObject();
            if (!gameObject)
                return layerMask == -1;

            return ((1 << gameObject.layer) & layerMask) != 0;
        }

        public override Object Find()
        {
            return Enumerate().FirstOrDefault();
        }

        public override Object[] FindAll()
        {
            return Enumerate().ToArray();
        }

        IEnumerable<GameObject> Enumerate()
        {
            foreach (var gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (Match(gameObject, false))
                    yield return gameObject;
        }
    }
}

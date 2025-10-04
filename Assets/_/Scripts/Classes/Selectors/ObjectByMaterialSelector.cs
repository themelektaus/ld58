using System.Linq;

using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ObjectByMaterialSelector : ObjectSelector
    {
        public Material material;

        public override bool Match(Object value, bool includeTree)
        {
            if (!value)
                return !material;

            var gameObject = value.GetGameObject();
            if (!gameObject)
                return !material;

            var renderer = gameObject.GetComponent<Renderer>();

            if (!renderer && includeTree)
                renderer = gameObject.GetComponentInChildren<Renderer>();

            if (!renderer && includeTree)
                renderer = gameObject.GetComponentInParent<Renderer>();

            if (!renderer)
                return !material;

            return Match(renderer);
        }

        bool Match(Renderer renderer)
        {
            return renderer.material.name.StartsWith(material.name);
        }

        public override Object Find()
        {
            return Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                         .FirstOrDefault(x => Match(x));
        }

        public override Object[] FindAll()
        {
            return Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                         .Where(x => Match(x))
                         .ToArray();
        }
    }
}

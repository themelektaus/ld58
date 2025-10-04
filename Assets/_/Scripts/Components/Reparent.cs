using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Reparent")]
    public class Reparent : MonoBehaviour
    {
        public string parentPath;

        [SerializeField] bool ignoreOwner;

        bool hasOwner;
        GameObject owner;

        void Awake()
        {
            var parent = transform.parent;
            if (!ignoreOwner && parent)
            {
                hasOwner = true;
                owner = parent.gameObject;
            }

            if (parentPath.IsNullOrEmpty())
            {
                transform.SetParent(null, worldPositionStays: true);
                return;
            }

            if (parentPath == "..")
            {
                transform.SetParent(transform.parent.parent, worldPositionStays: true);
                return;
            }

            var hierarchy = parentPath.Split('/');
            foreach (var node in hierarchy)
            {
                var n = node.Trim();
                if (n.IsNullOrEmpty())
                    continue;

                var currentParent = parent;
                if (currentParent)
                {
                    parent = currentParent.Find(n);
                }
                else
                {
                    var parentGameObject = GameObject.Find($"/{n}");
                    if (parentGameObject)
                        parent = parentGameObject.transform;
                }

                if (!parent)
                    parent = new GameObject(n).transform;

                parent.parent = currentParent;
            }

            transform.parent = parent;
        }

        void Update()
        {
            if (!hasOwner)
                return;

            if (owner)
                gameObject.SetActive(owner.activeInHierarchy);
            else
                gameObject.Destroy();
        }
    }
}

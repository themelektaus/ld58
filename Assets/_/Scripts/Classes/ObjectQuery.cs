using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [CreateAssetMenu(menuName = "Prototype/Object Query")]
    public class ObjectQuery : ScriptableObject
    {
        [SerializeReference]
        List<ObjectSelector> selectors;

#if UNITY_EDITOR
        enum Selector
        {
            _,
            ObjectByName,
            ObjectByTag,
            ObjectByLayer,
            ObjectByComponent,
            ObjectByMaterial
        }

        [SerializeField] Selector addSelector;

        void OnValidate()
        {
            switch (addSelector)
            {
                case Selector.ObjectByName:
                    selectors.Add(new ObjectByNameSelector());
                    break;

                case Selector.ObjectByTag:
                    selectors.Add(new ObjectByTagSelector());
                    break;

                case Selector.ObjectByLayer:
                    selectors.Add(new ObjectByLayerSelector());
                    break;

                case Selector.ObjectByComponent:
                    selectors.Add(new ObjectByComponentSelector());
                    break;

                case Selector.ObjectByMaterial:
                    selectors.Add(new ObjectByMaterialSelector());
                    break;
            }

            addSelector = Selector._;
        }
#endif

        [SerializeField] MatchRequirement matchRequirement;

        [SerializeField] bool logging = true;

        class Cache
        {
            public readonly List<Object> objects = new();
            public readonly Dictionary<System.Type, Component> component = new();
            public readonly Dictionary<System.Type, Component[]> components = new();
            public readonly Dictionary<System.Type, Component> children = new();
            public readonly Dictionary<System.Type, Component> parents = new();
        }

        Cache cache;

        protected override void Initialize()
        {
            ClearCache();
        }

        public void ClearCache()
        {
            cache = null;
        }

        public bool ClearCacheOf(Object @object)
        {
            if (Match(@object))
            {
                ClearCache();
                return true;
            }
            return false;
        }

        public bool Match(Object @object)
        {
            if (selectors.Count == 0)
                return true;

            var any = matchRequirement == MatchRequirement.Any;
            var includeTree = matchRequirement == MatchRequirement.SameTransformRoot;

            foreach (var selector in selectors)
            {
                if (selector.Match(@object, includeTree))
                {
                    if (any)
                        return true;

                    continue;
                }

                if (!any)
                    return false;
            }

            return !any;
        }

        public List<Object> FindObjects()
        {
            return _FindAll().objects;
        }

        public T[] FindObjects<T>() where T : Object
        {
            return FindObjects().Filter<T>().ToArray();
        }

        public IEnumerable<GameObject> FindGameObjects()
        {
            return FindObjects().Select(x => x.GetGameObject());
        }

        public Object FindObject()
        {
            return FindObjects().FirstOrDefault();
        }

        public GameObject FindGameObject()
        {
            var @object = FindObject();
            if (!@object)
                return null;
            return @object.GetGameObject();
        }

        public Transform FindTransform()
        {
            var @object = FindObject();
            if (!@object)
                return null;
            return @object.GetTransform();
        }

        public Transform[] FindTransforms()
        {
            return FindObjects().Select(x => x.GetTransform()).ToArray();
        }

        public T FindObject<T>() where T : Component
        {
            return FindObjects().Filter<T>().FirstOrDefault();
        }

        public T FindInterface<T>() where T : class
        {
            return FindObjects().Filter<T>().FirstOrDefault();
        }

        public T FindComponent<T>() where T : Component
        {
            var t = typeof(T);
            if (!_FindAll().component.ContainsKey(t))
            {
                var gameObject = FindGameObject();
                cache.component.Add(t, gameObject ? gameObject.GetComponent<T>() : null);
            }
            return cache.component[t] as T;
        }

        public void Activate()
        {
            var go = FindGameObject();
            if (go) go.SetActive(true);
        }

        public void Deactivate()
        {
            var go = FindGameObject();
            if (go) go.SetActive(false);
        }

        public bool IsEnabled<T>() where T : Component
        {
            var c = FindComponent<T>();
            return c && c.IsEnabled();
        }

        public void Enable<T>() where T : Component
        {
            var c = FindComponent<T>();
            if (c) c.Enable();
        }

        public void Disable<T>() where T : Component
        {
            var c = FindComponent<T>();
            if (c) c.Disable();
        }

        public void DestroyGameObject()
        {
            var go = FindGameObject();
            if (go) go.Destroy();
        }

        public T[] FindComponents<T>() where T : Component
        {
            var t = typeof(T);
            if (!_FindAll().components.ContainsKey(t))
                cache.components.Add(t, FindGameObjects().Select(x => x.GetComponent<T>()).ToArray());
            return cache.components[t] as T[];
        }

        public T FindComponentInChildren<T>() where T : Component
        {
            var t = typeof(T);
            if (!_FindAll().children.ContainsKey(t))
            {
                var gameObject = FindGameObject();
                cache.children.Add(t, gameObject ? gameObject.GetComponentInChildren<T>() : null);
            }
            return cache.children[t] as T;
        }

        public T FindComponentInParent<T>() where T : Component
        {
            var t = typeof(T);
            if (!_FindAll().parents.ContainsKey(t))
            {
                var gameObject = FindGameObject();
                cache.parents.Add(t, gameObject ? gameObject.GetComponentInParent<T>() : null);
            }
            return cache.parents[t] as T;
        }

        Cache _FindAll()
        {
            if (cache is not null)
                return cache;

            static void Add<T>(Dictionary<T, int> objects, T @object)
            {
                if (!objects.ContainsKey(@object))
                    objects.Add(@object, 0);
                objects[@object]++;
            }

            cache = new();

            var objects = new Dictionary<Object, int>();
            var gameObjects = new Dictionary<GameObject, int>();
            var transformList = new List<Transform>();

            foreach (var selector in selectors)
            {
                foreach (var @object in selector.FindAll())
                {
                    Add(objects, @object);

                    if (matchRequirement == MatchRequirement.SameGameObject)
                    {
                        Add(gameObjects, @object.GetGameObject());
                        continue;
                    }

                    if (matchRequirement == MatchRequirement.SameTransformRoot)
                    {
                        transformList.Add(@object.GetTransform());
                        continue;
                    }
                }
            }

            if (matchRequirement == MatchRequirement.Any)
            {
                cache.objects.AddRange(objects.Keys);
                goto Exit;
            }

            if (matchRequirement == MatchRequirement.SameObject)
            {
                cache.objects.AddRange(objects.Where(x => x.Value == selectors.Count).Select(x => x.Key));
                goto Exit;
            }

            if (matchRequirement == MatchRequirement.SameGameObject)
            {
                cache.objects.AddRange(gameObjects.Where(x => x.Value == selectors.Count).Select(x => x.Key as Object));
                goto Exit;
            }

            if (matchRequirement == MatchRequirement.SameTransformRoot)
            {
                var transforms = new Dictionary<Transform, int>();

                foreach (var transform in transformList)
                    Add(transforms, Root(transform, transformList));

                cache.objects.AddRange(transforms.Where(x => x.Value == selectors.Count).Select(x => x.Key as Object));
                goto Exit;
            }

            throw new();

        Exit:
            if (logging)
            {
                if (Monitor.queries.ContainsKey(this))
                    Monitor.queries[this] = cache.objects;
                else
                    Monitor.queries.Add(this, cache.objects);

            }

            return cache;
        }

        Transform Root(Transform root, IEnumerable<Transform> transforms)
        {
            foreach (var transform in transforms)
            {
                if (root == transform || transform.IsChildOf(root))
                    continue;

                if (root.IsChildOf(transform))
                {
                    root = transform;
                    continue;
                }
            }
            return root;
        }

#if UNITY_EDITOR
        [Button, SerializeField] bool _Ping;
        void Ping()
        {
            UnityEditor.EditorGUIUtility.PingObject(FindObject());
        }

        [Button, SerializeField] bool _Select_All;
        void Select_All()
        {
            UnityEditor.Selection.objects = FindGameObjects().ToArray();
        }

        [Button, SerializeField] bool _Clear_Cache;
        void Clear_Cache()
        {
            ClearCache();
        }
#endif
    }
}

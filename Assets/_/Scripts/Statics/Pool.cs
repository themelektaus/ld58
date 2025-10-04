using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    public static class Pool
    {
        public class Entry
        {
            public class Instance
            {
                public GameObject gameObject;
                public bool destroyed;

                readonly GameObject original;

                public Instance(GameObject original) : this(original, null)
                {

                }

                public Instance(GameObject original, Transform parent)
                {
                    this.original = original;
                    gameObject = Object.Instantiate(original);
                    Reset(parent);
                }

                public void Reset(Transform parent)
                {
                    gameObject.SetActive(true);

                    var gameObjectTransform = gameObject.transform;
                    if (parent)
                        gameObjectTransform.parent = parent;

                    var originalTransform = original.transform;
                    gameObjectTransform.localPosition = originalTransform.localPosition;
                    gameObjectTransform.localRotation = originalTransform.localRotation;
                    gameObjectTransform.localScale = originalTransform.localScale;

                    destroyed = false;
                }

                public void Destroy()
                {
                    gameObject.SetActive(false);
                    destroyed = true;
                }
            }

            public GameObject original;
            public List<Instance> instances = new();

            public Entry(GameObject original)
            {
                this.original = original;
            }

            public Instance GetInstance(Transform parent)
            {
                var instance = instances.FirstOrDefault(x => x.destroyed);
                if (instance is null)
                {
                    instance = new(original, parent);
                    instances.Add(instance);
                }
                else
                {
                    instance.Reset(parent);
                }
                return instance;
            }
        }

        static readonly List<Entry> entries = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear()
        {
            if (entries is not null)
                entries.Clear();
        }

        public static GameObject Instantiate(GameObject original, Transform parent)
        {
            var entry = entries.FirstOrDefault(x => x.original == original);
            if (entry is null)
            {
                entry = new(original);
                entries.Add(entry);
            }
            return entry.GetInstance(parent).gameObject;
        }

        public static void Destroy(GameObject gameObject)
        {
            foreach (var entry in entries)
                foreach (var instance in entry.instances)
                    if (instance.gameObject == gameObject)
                        instance.Destroy();
        }
    }
}

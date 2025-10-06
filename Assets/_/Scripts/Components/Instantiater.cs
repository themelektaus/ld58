using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Instantiater")]
    public class Instantiater : MonoBehaviour
    {
        [SerializeField] GameObject prefab;
        [SerializeField] Object parent;

        [SerializeField] UnityEvent onInstantiate;
        [SerializeField] bool delayOnDestroy;
        [SerializeField] UnityEvent onDestroy;

        readonly List<GameObject> instances = new();

        public void Instantiate()
        {
            InstantiateGameObject();
        }

        public GameObject InstantiateGameObject()
        {
            return prefab.Instantiate(parent.GetTransform(), x =>
            {
                onInstantiate.Invoke();

                x.AddComponent<OnDestroy_>().AddListener(
                    delay: delayOnDestroy ? 1 : 0,
                    delayByFrameCount: delayOnDestroy,
                    () =>
                    {
                        instances.Remove(x);
                        onDestroy.Invoke();
                    }
                );

                instances.Add(x);
            });
        }

        public bool HasAnyInstance() => instances.Count > 0;

        public void DestroyInstances()
        {
            foreach (var instance in instances.ToList())
                instance.Destroy();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var count = onDestroy.GetPersistentEventCount();

            var @this = new UnityEditor.SerializedObject(this);

            for (int i = 0; i < count; i++)
            {
                var target = onDestroy.GetPersistentTarget(i);
                if (target is not SceneLoader sceneLoader)
                    continue;

                var methodName = onDestroy.GetPersistentMethodName(i);
                if (methodName != nameof(sceneLoader.Load))
                    continue;

                var stringArgument = @this.FindProperty(
                    $"onDestroy.m_PersistentCalls.m_Calls.Array.data[{i}].m_Arguments.m_StringArgument"
                );

                this.DrawEditorLabel(
                    "Target",
                    stringArgument.stringValue,
                    options: new() { offset = new(0, -.25f) }
                );
            };
        }
#endif
    }
}

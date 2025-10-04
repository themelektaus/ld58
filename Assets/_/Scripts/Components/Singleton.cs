using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Singleton")]
    public class Singleton : MonoBehaviour
    {
        static readonly Dictionary<string, Singleton> instances = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() => instances.Clear();

        [SerializeField, ReadOnly] string key;

        [SerializeField] bool dontDestroy;

        void Awake()
        {
            if (instances.ContainsKey(key))
            {
                gameObject.Kill();
                return;
            }

            instances.Add(key, this);

            if (dontDestroy)
            {
                this.DontDestroy();
            }
        }

        void OnDestroy()
        {
            if (instances.TryGetValue(key, out var singleton) && singleton == this)
            {
                instances.Remove(key);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (!currentStage)
            {
                return;
            }

            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (stage != currentStage)
            {
                return;
            }

            var key = stage.assetPath;
            if (this.key == key)
            {
                return;
            }

            this.key = key;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}

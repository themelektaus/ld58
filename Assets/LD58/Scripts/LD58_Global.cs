using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Prototype.LD58
{
    public class LD58_Global : UnityEngine.ScriptableObject
    {
        [SerializeField] bool autoInitialize = true;

        public Singletons singletons;

        [Serializable]
        public struct Singletons
        {
            public Camera mainCamera;
            public Camera uiCamera;
            public EventSystem eventSystem;
            public LD58_GameStateMachine gameStateMachine;
            public Framerate framerate;
            public PostProcessVolume postProcessVolume;
            public LD58_Cursor cursor;
        }

        public readonly Settings settings = new();
        public class Settings
        {
            public bool vSync
            {
                get => GetSingletonInstance(x => x.framerate).vSync;
                set => GetSingletonInstance(x => x.framerate).vSync = value;
            }

            public bool fullscreen
            {
                get => GetSingletonInstance(x => x.framerate).fullscreen;
                set => GetSingletonInstance(x => x.framerate).fullscreen = value;
            }
        }

        static LD58_Global _instance;
        public static LD58_Global instance => _instance
            ? _instance
            : _instance = Utils.GetResource<LD58_Global>(nameof(LD58_Global));

        public static readonly List<RuntimeSingleton> runtimeSingletons = new();

        public class RuntimeSingleton
        {
            public GameObject prefab;
            public GameObject instance;
            public Func<IEnumerable<GameObject>> getOthers;

            public void Instantiate()
            {
                instance = prefab.Instantiate().DontDestroy();
                instance.name = prefab.name;
                instance.transform.SetLocalPositionAndRotation(
                    prefab.transform.localPosition,
                    prefab.transform.localRotation
                );
            }

            public void Clear()
            {
                foreach (var other in getOthers())
                {
                    if (other != instance)
                    {
                        other.Kill();
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            runtimeSingletons.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;

            if (!instance.autoInitialize)
            {
                return;
            }

            SceneManager.sceneLoaded += SceneManager_SceneLoaded;

            AddSingleton(
                instance.singletons.mainCamera,
                () => FindObjectsByType<Camera>(FindObjectsSortMode.None)
                    .Where(x => x.CompareTag("MainCamera"))
            );

            AddSingleton(
                instance.singletons.uiCamera,
                () => FindObjectsByType<Camera>(FindObjectsSortMode.None)
                    .Where(x => x.name == "UI Camera")
            );

            AddSingleton(
                instance.singletons.eventSystem,
                () => FindObjectsByType<EventSystem>(FindObjectsSortMode.None)
            );

            AddSingleton(
                instance.singletons.gameStateMachine,
                () => FindObjectsByType<LD58_GameStateMachine>(FindObjectsSortMode.None)
            );

            AddSingleton(
                instance.singletons.framerate,
                () => FindObjectsByType<Framerate>(FindObjectsSortMode.None)
            );

            AddSingleton(
                instance.singletons.postProcessVolume,
                () => FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None)
                    .Where(x => x.name == "Post-process Volume")
            );

            AddSingleton(
                instance.singletons.cursor,
                () => FindObjectsByType<LD58_Cursor>(FindObjectsSortMode.None)
            );

            foreach (var runtimeSingleton in runtimeSingletons)
            {
                runtimeSingleton.Clear();
                runtimeSingleton.Instantiate();
            }
        }

        static void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            foreach (var runtimeSingleton in runtimeSingletons)
            {
                runtimeSingleton.Clear();
            }
        }

        static void AddSingleton(
            UnityEngine.Object prefab,
            Func<IEnumerable<UnityEngine.Object>> getOthers
        )
        {
            if (prefab)
            {
                var prefabGameObject = prefab.GetGameObject();
                if (prefabGameObject)
                {
                    runtimeSingletons.Add(new()
                    {
                        prefab = prefabGameObject,
                        getOthers = () => getOthers().Select(x => x.GetGameObject())
                    });
                }
            }
        }

        public static T GetSingletonInstance<T>(Func<Singletons, T> getPrefab) where T : Component
        {
            var prefab = getPrefab(instance.singletons).GetGameObject();
            var runtimeSingleton = runtimeSingletons.FirstOrDefault(x => x.prefab == prefab);
            return ExtensionMethods.GetComponent<T>(runtimeSingleton.instance);
        }
    }
}

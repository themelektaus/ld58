using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Prototype.LD58
{
    [CreateAssetMenu(menuName = "LD58: Global")]
    public class LD58_Global : UnityEngine.ScriptableObject
    {
        [SerializeField] bool autoInitialize = true;

        [SerializeField] ObjectQuery gameStateMachineQuery;
        public static LD58_GameStateMachine gameStateMachine
            => instance.gameStateMachineQuery.FindComponent<LD58_GameStateMachine>();

        [SerializeField] ObjectQuery framerateQuery;
        public static Framerate framerate
            => instance.framerateQuery.FindComponent<Framerate>();

        [SerializeField] ObjectQuery mainCameraQuery;
        public static Camera mainCamera
            => instance.mainCameraQuery.FindComponent<Camera>();

        [SerializeField] ObjectQuery uiCameraQuery;
        public static Camera uiCamera
            => instance.uiCameraQuery.FindComponent<Camera>();

        [SerializeField] Singletons singletons;
        [Serializable]
        struct Singletons
        {
            public Camera mainCamera;
            public Camera uiCamera;
            public EventSystem eventSystem;
            public LD58_GameStateMachine gameStateMachine;
            public Framerate framerate;
            public PostProcessVolume postProcessVolume;
            public GameMouseCursor gameMouseCursor;
        }

        public InterpolationCurve.InterpolationCurve easeCurve;

        public readonly Settings settings = new();
        public class Settings
        {
            public bool vSync
            {
                get => framerate.vSync;
                set => framerate.vSync = value;
            }

            public bool fullscreen
            {
                get => framerate.fullscreen;
                set => framerate.fullscreen = value;
            }
        }

        static LD58_Global _instance;
        public static LD58_Global instance => _instance
            ? _instance
            : _instance = Utils.GetResource<LD58_Global>(nameof(LD58_Global));

        static readonly List<RuntimeSingleton> runtimeSingletons = new();
        class RuntimeSingleton
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
                    if (other != instance)
                        other.Kill();
            }
        }

        public UnityEngine.Audio.AudioMixer audioMixer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            runtimeSingletons.Clear();

            if (instance.autoInitialize)
            {
                instance.Resume();
            }
        }

        public bool IsPaused()
        {
            return pause;
        }

        public void Pause()
        {
            pause = true;
        }

        public void Resume()
        {
            pause = false;
        }

        public void Retry()
        {
            gameStateMachine.Trigger("Switch Level");
            SceneSwitcher.GotoScene("Ingame", "Ingame", 1);
        }

        public void ResetGame()
        {
            foreach (var collectCounter in data.collectCounters)
            {
                collectCounter.amount = 0;
            }

            data.malus = 0;
            data.upgrade.speed.current = 0;
            data.upgrade.radius.current = 0;
            data.upgrade.mass.current = 0;
            data.upgrade.maxObjects.current = 0;
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
                    .Where(x => x.name == "Directional Light")
            );

            AddSingleton(
                instance.singletons.postProcessVolume,
                () => FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None)
                    .Where(x => x.name == "Post-process Volume")
            );

            AddSingleton(
                instance.singletons.gameMouseCursor,
                () => FindObjectsByType<GameMouseCursor>(FindObjectsSortMode.None)
            );

            foreach (var singleton in runtimeSingletons)
            {
                singleton.Clear();
                singleton.Instantiate();
            }
        }

        static void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            foreach (var singleton in runtimeSingletons)
            {
                singleton.Clear();
            }
        }

        static void AddSingleton(
            UnityEngine.Object singleton,
            Func<IEnumerable<UnityEngine.Object>> getOthers
        )
        {
            if (!singleton)
                return;

            var prefab = singleton.GetGameObject();
            if (!prefab)
                return;

            runtimeSingletons.Add(new()
            {
                prefab = prefab,
                getOthers = () => getOthers().Select(x => x.GetGameObject())
            });
        }

        [SerializeField] bool pause;

        public bool IsLevelEqual(int level)
        {
            return data.GetLevel() == level;
        }

        public bool IsLevelEqualOrBigger(int level)
        {
            return data.GetLevel() >= level;
        }

        public bool IsLevelLess(int level)
        {
            return data.GetLevel() < level;
        }

        public Data data;

        [Serializable]
        public class Data
        {
            public int[] levelValues;

            public List<CollectCounter> collectCounters;
            public int malus;

            public void Collect(TrashObject.Type type)
            {
                collectCounters.FirstOrDefault(x => x.type == type).amount++;
            }

            public void Malus(TrashObject.Type type)
            {
                malus += collectCounters.FirstOrDefault(x => x.type == type).value;

            }

            public int GetLevel()
            {
                var totalValue = collectCounters.Sum(x => x.amount * x.value) - malus;

                var level = -1;
                foreach (var levelValue in levelValues)
                {
                    if (levelValue <= totalValue)
                    {
                        level++;
                    }
                }

                return level;
            }

            [Serializable]
            public class CollectCounter
            {
                public TrashObject.Type type;
                public int amount;
                public int value;
            }

            public Upgrade upgrade;

            [Serializable]
            public class Upgrade
            {
                [Serializable]
                public class Level
                {
                    public int current;
                    public float[] values;

                    public static implicit operator float(Level level)
                    {
                        return level.values.Length > 0
                            ? level.values[Mathf.Clamp(level.current, 0, level.values.Length - 1)]
                            : 0;
                    }
                }

                public Level speed;
                public Level radius;
                public Level mass;
                public Level maxObjects;
            }
        }

        public void Upgrade(int index)
        {
            switch (index)
            {
                case 0:
                    data.upgrade.speed.current++;
                    break;

                case 1:
                    data.upgrade.radius.current++;
                    break;

                case 2:
                    data.upgrade.mass.current++;
                    break;

                case 3:
                    data.upgrade.maxObjects.current++;
                    break;
            }
        }
    }
}

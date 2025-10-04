using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Scene Switcher")]
    public class SceneSwitcher : MonoBehaviour
    {
        public string activeSceneName = string.Empty;
        public int loadingSceneIndex = -1;
        public List<string> scenesToUnload = new();
        public List<string> scenesToLoad = new();
        public UnityEvent onFinish = new();

        bool busy;
        AsyncOperation process;
        Dummy loader;
        LoadingScene loadingScene;

        public static void GotoScene(string from, string to, int loadingSceneIndex)
        {
            var dummy = Dummy.Create(nameof(SceneSwitcher)).DontDestroy();

            var switcher = dummy.gameObject.AddComponent<SceneSwitcher>();
            switcher.activeSceneName = to;
            switcher.loadingSceneIndex = loadingSceneIndex;
            switcher.scenesToUnload.Add(from);
            switcher.scenesToLoad.Add(to);
            switcher.onFinish.AddListener(dummy.gameObject.Destroy);
            switcher.PerformSwitch();
        }

        public void PerformSwitch()
        {
            if (busy)
            {
                this.LogWarning("busy");
                return;
            }

            busy = true;

            Begin();
        }

        void Begin()
        {
            if (loadingSceneIndex == -1)
            {
                var instant = Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(i => SceneManager.GetSceneAt(i).name)
                    .All(x => scenesToUnload.Contains(x));

                if (instant)
                {
                    for (var i = 0; i < scenesToLoad.Count; i++)
                    {
                        if (i == 0)
                        {
                            process = SceneManager.LoadSceneAsync(scenesToLoad[i]);
                            process.allowSceneActivation = true;
                        }
                        else
                        {
                            SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
                        }
                    }
                    onFinish.Invoke();
                    return;
                }

                Unload();
                return;
            }

            loader = Dummy.Create(nameof(SceneSwitcher) + " Loader").DontDestroy();

            process = SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
            process.completed += _ =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(loadingSceneIndex));

                loadingScene = FindAnyObjectByType<LoadingScene>();

                loader.While(() => !loadingScene.ReadyForLoading).Start(Unload);
            };
        }

        void Unload()
        {
            if (scenesToUnload.Count == 0)
            {
                Load();
                return;
            }

            var counter = 0;
            foreach (var sceneName in scenesToUnload)
            {
                process = SceneManager.UnloadSceneAsync(sceneName);
                process.completed += _ =>
                {
                    counter++;
                    if (counter == scenesToUnload.Count)
                    {
                        Load();
                    }
                };
            }
        }

        void Load()
        {
            if (scenesToLoad.Count == 0)
            {
                End();
                return;
            }

            var counter = 0;
            foreach (var sceneName in scenesToLoad)
            {
                process = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                process.completed += _ =>
                {
                    counter++;
                    if (counter == scenesToUnload.Count)
                    {
                        End();
                    }
                };
            }
        }

        void End()
        {
            if (activeSceneName != string.Empty)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeSceneName));
            }

            if (loadingSceneIndex == -1)
            {
                onFinish.Invoke();
                return;
            }

            loadingScene.LoadingDone = true;

            loader.While(() => !loadingScene.Exited)
                .Start(() =>
                {
                    process = SceneManager.UnloadSceneAsync(loadingSceneIndex);
                    process.completed += _ =>
                    {
                        loader.gameObject.Destroy();
                        onFinish.Invoke();
                    };
                });
        }
    }
}

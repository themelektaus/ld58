using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Scene Loader")]
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] bool dontDestroy;
        public UnityEvent<string> onLoad = new();
        public UnityEvent<string> onUnload = new();

        void Awake()
        {
            if (dontDestroy)
                this.DontDestroy();
        }

        public void Load(string sceneName)
        {
            var process = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            process.completed += _ => onLoad.Invoke(sceneName);
        }

        public void Unload(string sceneName)
        {
            var process = SceneManager.UnloadSceneAsync(sceneName);
            process.completed += _ => onUnload.Invoke(sceneName);
        }

        public void UnloadActive()
        {
            Unload(SceneManager.GetActiveScene().name);
        }

        public void SetActive(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(scene);
        }
    }
}

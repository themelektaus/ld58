using UnityEngine;

namespace Prototype
{
    public class GotoScene : MonoBehaviour
    {
        public string sceneName;
        public int loadingSceneIndex = -1;

        [SerializeField] float delay;

        public void Apply()
        {
            if (LoadingScene.current)
            {
                LoadingScene.current.LogWarning(nameof(LoadingScene.current));
                return;
            }

            var from = gameObject.scene.name;

            Dummy.Create(nameof(GotoScene))
                .DontDestroy()
                .Wait(delay)
                .Then(() => SceneSwitcher.GotoScene(
                    from,
                    sceneName,
                    loadingSceneIndex
                ))
                .DestroyOwnerGameObject()
                .Start();
        }
    }
}

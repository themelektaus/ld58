using UnityEngine;

namespace Prototype
{
    public abstract class GameStateInstance
    {
        public readonly GameStateMachine gameStateMachine;
        public readonly GameObject originalGameObject;
        public readonly Transform parent;

        public GameObject gameObject { get; private set; }

        bool hasPrefab => originalGameObject.scene.name is null;

        public GameStateInstance(
            GameStateMachine gameStateMachine,
            GameObject originalGameObject,
            Transform parent
        )
        {
            this.gameStateMachine = gameStateMachine;
            this.originalGameObject = originalGameObject;
            this.parent = parent;

            gameObject = originalGameObject;

            if (hasPrefab)
            {
                gameObject = gameObject.Instantiate();
                return;
            }

            gameObject.SetActive(true);
        }

        public void Destroy()
        {
            if (hasPrefab)
            {
                gameObject.Destroy();
                return;
            }

            gameObject.SetActive(false);
        }
    }
}

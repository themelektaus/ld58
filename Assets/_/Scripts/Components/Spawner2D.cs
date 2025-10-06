using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Spawner (2D)")]
    public class Spawner2D : MonoBehaviour
    {
        [SerializeField] int maxGameObjects = -1;
        [SerializeField] int budget = -1;
        [SerializeField] bool destroyOnDisable;
        [SerializeField] List<SpawnerObject> objects;
        [SerializeField] Vector2 interval = Vector2.one;

        public Collider2D area;

        [System.Serializable]
        public struct Obstacle
        {
            public Object @object;
            public float radius;
        }
        [SerializeField] List<Obstacle> obstacles = new();

        [SerializeField] UnityEvent onFinish;

        readonly Timer timer = new();

        readonly HashSet<GameObject> gameObjects = new();

        void OnEnable()
        {
            timer.Reset(clearTime: false);
        }

        void OnDisable()
        {
            if (!destroyOnDisable)
                return;

            foreach (var gameObject in gameObjects)
                if (gameObject)
                    gameObject.Destroy();

            gameObjects.Clear();
        }

        void Update()
        {
            gameObjects.RemoveWhere(x => !x);

            if (budget == 0)
            {
                if (gameObjects.Count == 0)
                {
                    onFinish.Invoke();
                    enabled = false;
                }
                return;
            }

            if (maxGameObjects > -1 && gameObjects.Count >= maxGameObjects)
                return;

            if (!timer.Update(interval, clearTime: false))
                return;

            Spawn();
        }

        void Spawn()
        {
            int tries = 0;
            int maxTries = 10;

        Retry:
            var position = Utils.RandomPointInsideCollider(area);
            foreach (var obstacle in obstacles)
            {
                if (!obstacle.@object)
                    continue;

                var transform = obstacle.@object.GetTransform();
                if (!transform)
                    continue;

                if (((Vector2) transform.position - position).sqrMagnitude < obstacle.radius * obstacle.radius)
                {
                    if (tries == maxTries)
                        throw new($"{nameof(tries)} == {maxTries}");

                    tries++;

                    goto Retry;
                }
            }

            var @object = Utils.RandomPick(objects);

            var gameObject = @object.gameObject.Instantiate(position);
            if (!gameObjects.Add(gameObject))
            {
                // MyTODO: What the hell, why?
                //gameObject.Kill();
                //throw new("Can not add spawned game object");
            }

            budget--;

            if (@object.lifetime < 0)
            {
                gameObject.RemoveComponent<Destroy>();
                return;
            }

            if (@object.lifetime == 0)
            {
                gameObject.GetOrAddComponent<Destroy>(x => { x.delayByFrameCount = true; x.delay = 1; });
                return;
            }

            gameObject.GetOrAddComponent<Destroy>(x => x.delay = @object.lifetime);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    public abstract class GameStateInstances<T> where T : GameStateInstance
    {
        readonly GameStateMachine gameStateMachine;

        readonly HashSet<T> instances = new();

        public GameStateInstances(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }

        public T Add(GameObject originalGameObject, Transform parent = null, bool active = true)
        {
            var instance = Activator.CreateInstance(typeof(T), new object[] { gameStateMachine, originalGameObject, parent }) as T;

            if (!active)
                instance.gameObject.SetActive(false);

            instances.Add(instance);

            if (instance.gameObject.TryGetComponent(out IGameState<T> x))
                x.gameStateInstance = instance;

            if (parent)
                instance.gameObject.transform.SetParent(parent, false);

            return instance;
        }

        public T Get(GameObject originalGameObject)
        {
            return instances.FirstOrDefault(x => x.originalGameObject == originalGameObject);
        }

        public void Remove(params GameObject[] originalGameObjects)
        {
            foreach (var originalGameObject in originalGameObjects)
            {
                var instance = Get(originalGameObject);
                if (instance is null)
                    continue;

                instance.Destroy();
                instances.Remove(instance);
            }
        }

        public void Clear(params GameObject[] except)
        {
            foreach (var instance in instances)
                if (!except.Contains(instance.originalGameObject))
                    instance.Destroy();

            instances.RemoveWhere(x => !except.Contains(x.originalGameObject));
        }
    }
}

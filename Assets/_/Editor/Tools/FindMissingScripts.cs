using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Prototype.Editor
{
    public static class FindMissingScripts
    {
        [MenuItem(Const.MENU_ASSETS + "/Find Missing Scripts")]
        public static void MenuItem()
        {
            var gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            var prefabs = new HashSet<Object>();
            int scriptCount = 0;
            int gameObjectCount = 0;

            var nestedGameObjects = gameObjects.SelectMany(x => x.GetComponentsInChildren<Transform>(true)).Select(x => x.gameObject);
            foreach (var gameObject in nestedGameObjects)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (count == 0)
                    continue;

                if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                {
                    RecursivePrefabSource(gameObject, prefabs, ref scriptCount, ref gameObjectCount);
                    count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                    if (count == 0)
                        continue;
                }
                scriptCount += count;
                gameObjectCount++;
            }

            Debug.Log($"Found {scriptCount} missing scripts from {gameObjectCount} GameObjects");
        }

        static void RecursivePrefabSource(GameObject gameObject, HashSet<Object> prefabs, ref int scriptCount, ref int gameObjectCount)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (source is null || !prefabs.Add(source))
                return;

            RecursivePrefabSource(source, prefabs, ref scriptCount, ref gameObjectCount);
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(source);
            if (count == 0)
                return;

            scriptCount += count;
            gameObjectCount++;
        }
    }
}

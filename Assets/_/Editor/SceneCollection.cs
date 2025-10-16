using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace Prototype.Editor
{
    [CreateAssetMenu(menuName = "Prototype/Scene Collection (Editor)")]
    public class SceneCollection : UnityEngine.ScriptableObject
    {
        public List<SceneAsset> sceneAssets = new();

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int _)
        {
            var @object = EditorUtility.InstanceIDToObject(instanceID);
            if (@object is SceneCollection sceneCollection)
                sceneCollection.Open();
            return false;
        }

        public void Open()
        {
            if (sceneAssets.Count == 0)
                return;

            OpenScene(sceneAssets.First(), OpenSceneMode.Single);

            foreach (var sceneAsset in sceneAssets.Skip(1))
                OpenScene(sceneAsset, OpenSceneMode.Additive);
        }

        static void OpenScene(SceneAsset sceneAsset, OpenSceneMode mode)
        {
            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            EditorSceneManager.OpenScene(scenePath, mode);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace QuickMenu
{
    public class SceneSelectorMenuItem : IMenuItem
    {
        public bool visible => true;
        public int priority => 0;

        public string title => scene.sceneAsset.name;

        public string description => scene.path;

        public string category => null;

        public string subCategory => null;

        public struct Scene
        {
            public UnityEditor.SceneAsset sceneAsset;
            public string path;
        }

        readonly Scene scene;
        readonly Action<Scene> command;

        public SceneSelectorMenuItem(Scene scene, Action<Scene> command)
        {
            this.scene = scene;
            this.command = command;
        }

        public bool Command(Context context)
        {
            command(scene);
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }

        public bool Validation(Context context)
        {
            return true;
        }
    }
}

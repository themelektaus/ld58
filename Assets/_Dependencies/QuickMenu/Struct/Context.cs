using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickMenu
{
    public struct Context
    {
        public Object[] objects;
        public Object @object;
        public GameObject[] gameObjects;
        public GameObject gameObject;
        public Transform[] transforms;
        public Transform transform;

        public string[] assetGUIDs;
        public string assetGUID;

        public List<string> assetPaths;
        public string assetPath;
        public string assetFolder;

        public List<Object> assets;
        public Object asset;

        public List<Scene> scenes;
        public Scene scene;

        public WindowType focusedWindow; 
        public object userData;

        public bool isProjectBrowser => focusedWindow == WindowType.ProjectBrowser;
        public bool isProjectBrowserWithAsset => isProjectBrowser && asset is not null;
        public bool isSceneHierarchy => focusedWindow == WindowType.SceneHierarchy;
        public bool isAnimatorControllerTool => focusedWindow == WindowType.AnimatorControllerTool;
    }
}

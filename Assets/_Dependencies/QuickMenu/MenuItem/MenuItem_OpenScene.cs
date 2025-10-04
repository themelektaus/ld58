using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace QuickMenu
{
    public class MenuItem_OpenScene : IMenuItem
    {
        [AddQuickMenuItems]
        static IEnumerable<IMenuItem> _AddQuickMenuItems()
        {
            foreach (var sceneAsset in sceneAssets)
                yield return new MenuItem_OpenScene(sceneAsset.Key, sceneAsset.Value);
        }

        static Dictionary<SceneAsset, string> _sceneAssets;
        public static Dictionary<SceneAsset, string> sceneAssets
        {
            get
            {
                if (_sceneAssets is null)
                    _sceneAssets = new();

                if (_sceneAssets.Count == 0)
                {
                    var assets = AssetDatabase.FindAssets($"t:{nameof(SceneAsset)}")
                        .Select(x => AssetDatabase.GUIDToAssetPath(x))
                        .Select(x => (x, AssetDatabase.LoadAssetAtPath<SceneAsset>(x)));

                    foreach (var asset in assets)
                        _sceneAssets.Add(asset.Item2, asset.x);
                }

                return _sceneAssets;
            }
        }

        public bool visible => true;
        public int priority => 0;

        public string title => sceneAsset.name;
        public string description => sceneAssetPath;

        public string category => "Scene";
        public string subCategory => "Open";

        readonly SceneAsset sceneAsset;
        readonly string sceneAssetPath;

        public MenuItem_OpenScene(SceneAsset sceneAsset, string sceneAssetPath)
        {
            this.sceneAsset = sceneAsset;
            this.sceneAssetPath = sceneAssetPath;
        }

        public bool Validation(Context context)
        {
            if (context.isAnimatorControllerTool)
                return false;

            return true;
        }

        public bool Command(Context context)
        {
            AssetDatabase.OpenAsset(sceneAsset);
            return true;
        }

        public IEnumerable<VisualElement> GetParameterFields()
        {
            return null;
        }
    }
}
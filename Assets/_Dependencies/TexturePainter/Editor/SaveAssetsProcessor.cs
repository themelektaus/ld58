namespace TexturePainter.Editor
{
    public class SaveAssetsProcessor : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            var materialPropertyOverrides = UnityEngine.Object.FindObjectsByType<MaterialPropertyOverride.MaterialPropertyOverride>(UnityEngine.FindObjectsSortMode.None);
            foreach (var materialPropertyOverride in materialPropertyOverrides)
                materialPropertyOverride.Refresh();
            return paths;
        }
    }
}
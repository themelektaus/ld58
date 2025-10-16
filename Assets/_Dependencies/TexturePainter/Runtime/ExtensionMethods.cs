using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace TexturePainter
{
    public static class ExtensionMethods
    {
        public static GameObject GetGameObjectWithMaterial(this GameObject @this)
        {
            return @this.GetGameObjectWithMaterial(out _);
        }

        public static GameObject GetGameObjectWithMaterial(this GameObject @this, out Material material)
        {
            var (gameObject, sharedMaterial) = @this
                .GetComponentsInChildren<Renderer>()
                .Where(x => x.sharedMaterial)
                .Select(x => (x.gameObject, x.sharedMaterial))
                .FirstOrDefault();

            material = sharedMaterial;

            return gameObject;
        }

#if UNITY_EDITOR
        public static void Save(this Texture2D @this, string path)
        {
            System.IO.File.WriteAllBytes(path, @this.EncodeToPNG());
        }

        public static void Add<T>(this Object @this, T child, bool copy) where T : Object
        {
            var newChild = child;
            if (copy)
            {
                if (child is ScriptableObject)
                    newChild = ScriptableObject.CreateInstance(typeof(T)) as T;
                else
                    newChild = System.Activator.CreateInstance<T>();
            }

            EditorUtility.CopySerialized(child, newChild);
            AssetDatabase.AddObjectToAsset(newChild, @this);
        }

        public static void Delete(this Object @this)
        {
            AssetDatabase.RemoveObjectFromAsset(@this);
        }

        public static void WriteTo(this RenderTexture @this, Texture2D texture)
        {
            Graphics.SetRenderTarget(@this);
            texture.ReadPixels(new Rect(0, 0, @this.width, @this.height), 0, 0);
            texture.Apply();
        }
#endif
    }
}
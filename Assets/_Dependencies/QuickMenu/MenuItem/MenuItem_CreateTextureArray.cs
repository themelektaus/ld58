using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuickMenu
{
    public class MenuItem_CreateTextureArray : MenuItem
    {
        public override string title => "Create Texture Array";

        public string name = "";

        public override bool Validation(Context context)
        {
            if (context.objects.Length < 2)
            {
                return false;
            }

            foreach (var @object in context.objects)
            {
                if (@object is not Texture2D)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Command(Context context)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(context.assetFolder, name)
            ) + ".asset";

            var textures = context.objects.Select(x => x as Texture2D).OrderBy(x => x.name).ToList();
            var firstTexture = textures[0];
            
            var array = new Texture2DArray(
                firstTexture.width,
                firstTexture.height,
                textures.Count,
                firstTexture.format,
                mipCount: firstTexture.mipmapCount,
                linear: false
            );

            for (int i = 0; i < textures.Count; i++)
            {
                array.SetPixels32(textures[i].GetPixels32(0), i, 0);
            }

            array.Apply();

            AssetDatabase.CreateAsset(array, path);

            return true;
        }
    }
}

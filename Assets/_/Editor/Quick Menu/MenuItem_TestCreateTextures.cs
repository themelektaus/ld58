using QuickMenu;

using UnityEngine;

namespace Prototype.Editor
{
    public class MenuItem_TestCreateTextures : MenuItem
    {
        public override string title => "Test DontSave Create Textures";
        public override string description => "Create a bunch of textures with Hide Flags set to DontSave.";

        public bool dontSave = true;
        public int iterations = 100;
        public int sizeX = 1024;
        public int sizeY = 1024;

        public override bool visible => false;

        public override bool Validation(Context context)
        {
            return false;
        }

        public override bool Command(Context context)
        {
            for (int i = 0; i < iterations; i++)
            {
                var texture = new Texture2D(sizeX, sizeY);

                if (dontSave)
                    texture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

                for (int x = 0; x < sizeX; x++)
                    for (int y = 0; y < sizeY; y++)
                        texture.SetPixel(x, y, Color.white);

                texture.Apply();
            }

            return true;
        }
    }
}

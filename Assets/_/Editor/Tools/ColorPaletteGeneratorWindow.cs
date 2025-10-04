using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    public class ColorPaletteGeneratorWindow : EditorWindow
    {
        [SerializeField] List<Texture2D> textures;

        ScrollView content;
        Button generateButton;
        Button saveButton;

        Texture2D colorPalette;

        [MenuItem(Const.MENU_ASSETS + "/Generate Color Palette")]
        public static void Open()
        {
            GetWindow<ColorPaletteGeneratorWindow>("Color Palette Generator");
        }

        void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1;
            root.style.SetPadding(2);

            content = new() { style = { flexGrow = 1 } };
            root.Add(content);

            var bottom = new VisualElement { style = { flexShrink = 0 } };
            bottom.Add(generateButton = new(Generate) { text = "Generate" });
            bottom.Add(saveButton = new(Save) { text = "Save" });
            root.Add(bottom);

            OnSelectionChange();
        }

        void OnSelectionChange()
        {
            Refresh();
        }

        void Refresh()
        {
            textures = EnumerateSelectedTextures().ToList();

            generateButton.SetEnabled(textures.Any());
            saveButton.SetEnabled(false);

            content.Clear();
            foreach (var selection in textures)
            {
                var objectField = CreateObjectField(selection);
                content.Add(objectField);
            }
        }

        static ObjectField CreateObjectField(Object @object)
        {
            var objectField = new ObjectField()
            {
                objectType = @object.GetType(),
                value = @object
            };
            objectField.SetEnabled(false);
            return objectField;
        }

        void Generate()
        {
            var selectedObjects = Selection.objects;

            var rollbackActions = new List<RollbackAction>();

            foreach (var texture in textures)
                SetWriteable(in texture, in rollbackActions);
            AssetDatabase.Refresh();

            var colorSet = new HashSet<Color>();
            foreach (var texture in textures)
                UpdateColorPalette(in texture, ref colorSet);

            foreach (var rollbackAction in rollbackActions)
                rollbackAction.Invoke();
            AssetDatabase.Refresh();

            var colors = colorSet.OrderBy(x => x.GetHue()).ToList();

            if (colorPalette)
                DestroyImmediate(colorPalette);

            colorPalette = new(colors.Count, 1, TextureFormat.ARGB32, false);
            content.Clear();
            for (int i = 0; i < colors.Count; i++)
            {
                colorPalette.SetPixel(i, 0, colors[i]);
                content.Add(new ColorField() { value = colors[i] });
            }
            colorPalette.Apply();

            Selection.objects = selectedObjects;

            saveButton.SetEnabled(true);
        }

        void Save()
        {
            saveButton.SetEnabled(false);

            var path = System.IO.Path.Combine("Assets", "Color Palette.png");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var bytes = colorPalette.EncodeToPNG();

            System.IO.File.WriteAllBytes(path, bytes);
        }

        static IEnumerable<Texture2D> EnumerateSelectedTextures()
        {
            foreach (var selection in Selection.objects)
                if (selection is Texture2D texture)
                    yield return texture;
        }

        delegate void RollbackAction();

        static void SetWriteable(in Texture2D texture, in List<RollbackAction> rollbackActions)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer.isReadable)
                return;

            importer.isReadable = true;
            AssetDatabase.ImportAsset(assetPath);

            rollbackActions.Add(() =>
            {
                importer.isReadable = false;
                AssetDatabase.ImportAsset(assetPath);
            });
        }

        static void UpdateColorPalette(in Texture2D texture, ref HashSet<Color> colors)
        {
            var list = colors.ToList();
            list.AddRange(texture.GetPixels());
            colors = list.ToHashSet();
            colors.RemoveWhere(x => x.a == 0);
        }
    }
}

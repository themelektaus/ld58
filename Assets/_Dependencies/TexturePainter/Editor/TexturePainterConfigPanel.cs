using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace TexturePainter.Editor
{
    public class TexturePainterConfigPanel : VisualElement
    {
        public TexturePainterConfig config { get; private set; }

        public event System.Action<string> onPropertyChanged;

        SerializedObject configObject;

        GameObject gameObject;

        TextField propertyField;
        Button selectPropertyButton;
        GenericMenu selectPropertyMenu;

        // Custom magic method
        void OnCreate()
        {
            config = Utils.LoadAsset<TexturePainterConfig>();
            configObject = new(config);
            this.Bind(configObject);

            propertyField = this.Q<TextField>("PropertyField");
            propertyField.RegisterValueChangedCallback(x => onPropertyChanged?.Invoke(x.newValue));

            selectPropertyButton = this.Q<Button>("SelectPropertyButton");
            selectPropertyButton.clicked += () =>
                selectPropertyMenu?.ShowAsContext();
        }

        public void Select(GameObject gameObject)
        {
            if (this.gameObject == gameObject)
                return;

            this.gameObject = gameObject;

            Refresh();
        }

        public void Refresh()
        {
            selectPropertyButton.SetEnabled(false);

            selectPropertyMenu = new();
            if (!gameObject)
                return;

            gameObject.GetGameObjectWithMaterial(out var material);
            if (!material)
                return;

            var propertyNames = material.GetTexturePropertyNames();
            if (propertyNames.Length == 0)
                return;

            selectPropertyButton.SetEnabled(true);

            foreach (var propertyName in propertyNames)
            {
                selectPropertyMenu.AddItem(new GUIContent(propertyName), config.brush.property == propertyName, x =>
                {
                    config.brush.property = x as string;
                    configObject.Update();
                    Refresh();
                }, propertyName);
            }
        }
    }
}
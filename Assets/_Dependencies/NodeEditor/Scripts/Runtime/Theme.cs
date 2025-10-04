using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor
{
    [CreateAssetMenu(menuName = "Node Editor Theme")]
    public class Theme : ScriptableObject
    {
        static Theme[] themes = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitializeOnLoad() => themes = null;

        public static Theme @default
            => (themes ??= Resources.LoadAll<Theme>(""))
                .FirstOrDefault(x => x.isDefault);

        public static Theme active2
        {
            get
            {
                //themes ??= Resources.LoadAll<Theme>("");
                return null;// themes.FirstOrDefault(x => x.enabled);
            }
        }

        //public bool isEnabled;
        public bool isDefault;

        [Serializable]
        public struct Window
        {
            public Color titlebarColor;
            public Color backgroundColor;
            public Color textColor;
        }

        public Window window = new()
        {
            titlebarColor = new(.3f, .3f, 0.32f),
            backgroundColor = new(.2f, .2f, 0.21f),
            textColor = new(1, 1, 1)
        };

        [Serializable]
        public struct Button
        {
            public Color[] baseColors;

            public Color normalColor;
            public Color highlightedColor;
            public Color pressedColor;
            public Color selectedColor;
            [Range(1, 5)] public float colorMultiplier;
            public float fadeDuration;

            public Sprite rippleShape;
            [Range(0.1f, 5)] public float rippleSpeed;
            [Range(0.5f, 25)] public float rippleMaxSize;
            public Color rippleStartColor;
            public Color rippleTransitionColor;

            public void ApplyTo(CustomButton customButton)
            {
                var colorIndex = customButton.appearance.colorIndex;
                if (colorIndex > -1)
                    customButton.image.color = baseColors[colorIndex];

                var button = customButton.button;
                var colors = button.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = highlightedColor;
                colors.pressedColor = pressedColor;
                colors.selectedColor = selectedColor;
                colors.disabledColor = new(1, 1, 1);
                colors.colorMultiplier = colorMultiplier;
                colors.fadeDuration = fadeDuration;
                button.colors = colors;
            }

            public void CreateRipple(GameObject parent, Vector2 position)
            {
                parent.SetActive(true);
                parent.transform.SetAsFirstSibling();

                var gameObject = new GameObject("Ripple");
                gameObject.AddComponent<Image>().sprite = rippleShape;
                gameObject.transform.SetParent(parent.transform);
                gameObject.transform.position = position;

                var ripple = gameObject.AddComponent<Ripple>();
                ripple.speed = rippleSpeed;
                ripple.maxSize = rippleMaxSize;
                ripple.startColor = rippleStartColor;
                ripple.transitionColor = rippleTransitionColor;
            }
        }

        public Button button = new()
        {
            baseColors = new Color[]
            {
                new(.3f, .3f, .32f),
                new(0, .5f, .5f),
                new(.25f, .3f, .6f),
                new(.5f, .18f, .13f),
                new(.13f, .5f, .13f)
            },
            normalColor = new(1, 1, 1),
            highlightedColor = new(.85f, .85f, .85f),
            pressedColor = new(.8f, .8f, .8f),
            selectedColor = new(.85f, .85f, .85f),
            colorMultiplier = 1,
            fadeDuration = .1f,
            rippleSpeed = 2.4f,
            rippleMaxSize = 6,
            rippleStartColor = new(1, 1, 1, .05f),
            rippleTransitionColor = new(1, 1, 1, 0)
        };

        [Serializable]
        public struct ScrollView
        {
            public Color backgroundColor;
        }

        public ScrollView scrollView = new()
        {
            backgroundColor = new(.15f, .15f, .16f)
        };

        public TMPro.TMP_FontAsset defaultFont;
        public TMPro.TMP_FontAsset defaultFontSemiBold;
        public TMPro.TMP_FontAsset defaultFontBold;

        public float defaultFontSize = 21;
        public float outerPPU = 10;
        public float innerPPU = 15;
    }
}

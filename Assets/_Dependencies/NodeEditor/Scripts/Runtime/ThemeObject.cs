using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor
{
    [ExecuteAlways]
    public class ThemeObject : MonoBehaviour
    {
        public enum Type
        {
            None,
            Window,
            Dropdown,
            DropdownItem,
            ScrollView,
            ContextMenu,
            ContextMenuSeparator,
            Button
        }

        [SerializeField] Theme theme;
        [SerializeField] Type type;

        public Object[] objects;

        void Awake()
        {
            if (type == Type.None)
                return;

            if (!theme)
                theme = Theme.@default;

            if (type == Type.Window)
            {
                if (TryGet(0, out Image titlebarImage))
                {
                    titlebarImage.color = theme.window.titlebarColor;
                    titlebarImage.pixelsPerUnitMultiplier = theme.outerPPU;
                }

                if (TryGet(1, out Image backgroundImage))
                {
                    backgroundImage.color = theme.window.backgroundColor;
                    backgroundImage.pixelsPerUnitMultiplier = theme.outerPPU;
                }

                return;
            }

            if (type == Type.Dropdown)
            {
                if (TryGet(0, out Image image))
                {
                    image.color = theme.button.baseColors[0];
                    image.pixelsPerUnitMultiplier = theme.innerPPU;
                }

                if (TryGet(1, out TMPro.TextMeshProUGUI text))
                {
                    text.font = theme.defaultFontBold;
                    text.fontSize = theme.defaultFontSize;
                }

                return;
            }

            if (type == Type.DropdownItem)
            {
                if (TryGet(0, out TMPro.TextMeshProUGUI text))
                {
                    text.font = theme.defaultFont;
                    text.fontSize = theme.defaultFontSize;
                }
                return;
            }

            if (type == Type.ScrollView)
            {
                if (TryGet(0, out Image image))
                {
                    image.color = theme.scrollView.backgroundColor;
                }

                return;
            }

            if (type == Type.ContextMenu)
            {
                if (TryGet(0, out Image image))
                    image.color = theme.scrollView.backgroundColor;

                return;
            }

            if (type == Type.ContextMenuSeparator)
            {
                if (TryGet(0, out Image image))
                    image.color = theme.button.baseColors[0];

                return;
            }

            if (type == Type.Button)
            {
                if (TryGet(0, out Image image))
                    image.pixelsPerUnitMultiplier = theme.innerPPU;

                if (TryGet(1, out TMPro.TextMeshProUGUI text))
                {
                    text.font = theme.defaultFontSemiBold;
                    text.fontSize = theme.defaultFontSize;
                }

                if (TryGet(2, out image))
                    image.pixelsPerUnitMultiplier = theme.innerPPU;

                return;
            }
        }

        bool TryGet<T>(int index, out T @object)
        {
            if (index < objects.Length)
            {
                if (objects[index] is T result)
                {
                    @object = result;
                    return true;
                }
            }
            @object = default;
            return false;
        }
    }
}

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace TexturePainter.Editor
{
    public class TexturePainterToolbar : VisualElement
    {
        public ToolbarToggle paintModeToggle;

        public Mode mode
        {
            get => paintModeToggle.value ? Mode.Paint : Mode.None;
			set
            {
                var paintMode = false;

                switch (value)
                {
                    case Mode.Paint:
                        paintMode = true;
                        break;
                }

                if (paintModeToggle.value != paintMode)
                    paintModeToggle.value = paintMode;
            }
        }

        void OnCreate()
        {
            paintModeToggle = this.Q("PaintModeToggle") as ToolbarToggle;
            paintModeToggle.RegisterValueChangedCallback(e =>
            {
                if (!e.newValue)
                    return;

                mode = Mode.Paint;
                Tools.current = Tool.Custom;
            });
        }
    }
}
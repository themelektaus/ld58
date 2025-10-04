using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class Grid : VisualElement
    {
        public Grid()
        {
            for (int i = 0; i <= 10; i++)
            {
                var element = new VisualElement();
                element.AddToClassList("vertical-line");
                Add(element);
            }

            for (int i = 0; i <= 5; i++)
            {
                var element = new VisualElement();
                element.AddToClassList("horizontal-line");
                Add(element);
            }
        }

        public void Update(IAreaProvider areaProvider)
        {
            int index;

            index = 0;

            this.Query(className: "vertical-line").ForEach(x =>
            {
                x.style.left = areaProvider.GetAbsolute(new Vector2(index / 10f, 0)).x;
                index++;
            });

            index = 0;

            this.Query(className: "horizontal-line").ForEach(x =>
            {
                x.style.bottom = areaProvider.GetAbsolute(new Vector2(0, index / 5f)).y;
                index++;
            });
        }
    }
}

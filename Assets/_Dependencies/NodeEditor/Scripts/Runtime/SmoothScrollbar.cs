using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NodeEditor
{
    public class SmoothScrollbar : Scrollbar
    {
        SmoothScrollRect scrollRect;

        protected override void Awake()
        {
            base.Awake();

            scrollRect = transform.parent.GetComponentInChildren<SmoothScrollRect>();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!scrollRect)
                return;

            scrollRect.dragButtons.Add(eventData.button);
        }
    }
}

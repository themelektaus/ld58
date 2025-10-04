using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NodeEditor
{
    public class SmoothScrollRect : ScrollRect
    {
        Vector2 target;
        Vector2 currentVelocity;

        public readonly HashSet<PointerEventData.InputButton> dragButtons = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            ForceScrollToTop();
        }

        public void ForceScrollToTop()
        {
            target = new(1, 1);
            currentVelocity = new();
            normalizedPosition = new(0, 1);
        }

        protected virtual void Update()
        {
            dragButtons.RemoveWhere(x => Input.GetMouseButtonUp((int) x));

            if (!IsActive() || dragButtons.Count > 0)
            {
                target = normalizedPosition;
                currentVelocity = new();
                return;
            }

            normalizedPosition = Vector2.SmoothDamp(normalizedPosition, target, ref currentVelocity, .06f);
        }

        public override void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            var current = normalizedPosition;

            base.OnScroll(data);

            if (dragButtons.Count == 0)
            {
                target = normalizedPosition;
                normalizedPosition = current;
            }
        }

        public override void OnBeginDrag(PointerEventData _) { }
        public override void OnDrag(PointerEventData _) { }
        public override void OnEndDrag(PointerEventData _) { }
    }
}
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Pointer Down")]
    public class OnPointerDown_ : MonoBehaviour, IPointerDownHandler
    {
        public UnityEvent onLeftClick = new();
        public UnityEvent onRightClick = new();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabled)
            {
                this.LogWarning($"{nameof(OnPointerDown_)} not enabled");
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onLeftClick.Invoke();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onRightClick.Invoke();
                return;
            }
        }
    }
}

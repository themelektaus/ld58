using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Pointer Hover")]
    public class OnPointerHover : MonoBehaviour
    {
        [SerializeField] UnityEvent onHover;
        [SerializeField] UnityEvent onElse;

        void Update()
        {
            if (!enabled)
            {
                this.LogWarning($"{nameof(OnPointerHover)} not enabled");
            }

            if (Utils.IsPointerOver(gameObject))
            {
                onHover.Invoke();
                return;
            }

            onElse.Invoke();
        }
    }
}

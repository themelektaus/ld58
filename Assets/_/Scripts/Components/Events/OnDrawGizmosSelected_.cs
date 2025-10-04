#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Draw Gizmos Selected")]
    public class OnDrawGizmosSelected_ : MonoBehaviour
    {
        [SerializeField] UnityEvent @event;

        void OnDrawGizmosSelected()
        {
            if (!enabled)
            {
                this.LogWarning($"{nameof(OnDrawGizmosSelected_)} not enabled");
            }

            @event.Invoke();
        }
    }
}
#endif
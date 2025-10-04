using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Toggle (Linked)")]
    public class OnToggle : MonoBehaviour
    {
        [SerializeField] Object reference;

        [SerializeField] UnityEvent onEnable;
        [SerializeField] UnityEvent onDisable;

        bool? _enabled;

        void OnEnable()
        {
            _enabled = null;
            Update();
        }

        void OnDisable()
        {
            Update();
        }

        void Update()
        {
            var reference = this.reference;
            if (!reference)
                reference = this;

            var enabled = reference.IsActiveOrEnabled();

            if (_enabled.HasValue && _enabled.Value == enabled)
                return;

            _enabled = enabled;
            if (_enabled.Value)
                onEnable.Invoke();
            else
                onDisable.Invoke();
        }
    }
}

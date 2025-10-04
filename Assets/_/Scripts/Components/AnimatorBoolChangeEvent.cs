using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Animator Bool Change Event")]
    public class AnimatorBoolChangeEvent : MonoBehaviour
    {
        public class Args
        {
            public bool value;
        }

        [SerializeField] string parameter;
        [SerializeField] UnityEvent<Args> @event;

        Animator animator;
        bool value;

        void Awake()
        {
            animator = GetComponent<Animator>();
            value = GetBool();
        }

        bool GetBool()
        {
            return animator.GetBool(parameter);
        }

        void Update()
        {
            var newValue = GetBool();
            if (value == newValue)
                return;

            @event.Invoke(new() { value = newValue });
            value = newValue;
        }
    }
}

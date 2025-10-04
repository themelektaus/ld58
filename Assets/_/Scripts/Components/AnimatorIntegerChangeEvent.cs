using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Animator Integer Change Event")]
    public class AnimatorIntegerChangeEvent : MonoBehaviour
    {
        public class Args
        {
            public int value;
        }

        [SerializeField] string parameter;
        [SerializeField] UnityEvent<Args> @event;

        Animator animator;
        int value;

        void Awake()
        {
            animator = GetComponent<Animator>();
            value = GetInteger();
        }

        int GetInteger()
        {
            return animator.GetInteger(parameter);
        }

        void Update()
        {
            var newValue = GetInteger();
            if (value == newValue)
                return;

            @event.Invoke(new() { value = newValue });
            value = newValue;
        }
    }
}

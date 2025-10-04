using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Animator Float Change Event")]
    public class AnimatorFloatChangeEvent : MonoBehaviour
    {
        public class Args
        {
            public float value;
        }

        [SerializeField] string parameter;
        [SerializeField] UnityEvent<Args> @event;

        Animator animator;
        float value;

        void Awake()
        {
            animator = GetComponent<Animator>();
            value = GetFloat();
        }

        float GetFloat()
        {
            return animator.GetFloat(parameter);
        }

        void Update()
        {
            var newValue = GetFloat();
            if (value == newValue)
                return;

            @event.Invoke(new() { value = newValue });
            value = newValue;
        }
    }
}

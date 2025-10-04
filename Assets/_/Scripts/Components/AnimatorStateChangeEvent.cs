using UnityEngine;
using UnityEngine.Events;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Animator State Change Event")]
    public class AnimatorStateChangeEvent : MonoBehaviour
    {
        public class Args
        {
            public Type type;
        }

        public enum Type { Enter, Exit }

        // MyTODO: Make layerIndex to an integer
        //         it's a string because of the UIElements bindingPath
        //         on the dropdown (it only allows a list of strings)
        [SerializeField] string layerIndex = "0";

        [SerializeField] string state;
        [SerializeField] Type type;
        [SerializeField] UnityEvent<Args> callbacks;

        Animator animator;
        int _layerIndex;
        AnimatorStateInfo _state;

        AnimatorStateInfo GetAnimatorState()
        {
            return animator.GetCurrentAnimatorStateInfo(_layerIndex);
        }

        void Awake()
        {
            animator = GetComponent<Animator>();
            _layerIndex = int.Parse(layerIndex);
            _state = GetAnimatorState();
        }

        void Update()
        {
            var newState = GetAnimatorState();

            if (_state.fullPathHash == newState.fullPathHash)
                return;

            InvokeEvent(_state, Type.Exit);
            InvokeEvent(newState, Type.Enter);
            _state = newState;
        }

        void InvokeEvent(AnimatorStateInfo state, Type type)
        {
            if (this.type == type && state.IsName(this.state))
                callbacks.Invoke(new() { type = type });
        }
    }
}

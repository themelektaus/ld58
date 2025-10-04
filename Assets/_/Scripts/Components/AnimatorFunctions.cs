using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Animator Functions")]
    public class AnimatorFunctions : MonoBehaviour
    {
        [SerializeField] string context;

        Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetTrigger()
        {
            animator.SetTrigger(context);
        }

        public void SetBool(bool value)
        {
            animator.SetBool(context, value);
        }

        public void SetInteger(int value)
        {
            animator.SetInteger(context, value);
        }

        public void SetFloat(float value)
        {
            animator.SetFloat(context, value);
        }

        public float GetWeight()
        {
            int index = animator.GetLayerIndex(context);
            return animator.GetLayerWeight(index);
        }

        public void SetWeight(float value)
        {
            int index = animator.GetLayerIndex(context);
            animator.SetLayerWeight(index, value);
        }
    }
}

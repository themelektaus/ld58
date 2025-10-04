using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Top-Down Character Animator (2D)")]
    public class TopDownCharacterAnimator2D : MonoBehaviour
    {
        [SerializeField] string moveLayer = "Move Layer";
        [SerializeField] string horizontalParameter = "Horizontal";
        [SerializeField] string verticalParameter = "Vertical";

        Animator animator;
        TopDownCharacterController2D controller;

        void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponentInParent<TopDownCharacterController2D>();
        }

        void Update()
        {
            var moveLayerIndex = animator.GetLayerIndex(moveLayer);
            animator.SetLayerWeight(moveLayerIndex, controller.isMoving ? 1 : 0);

            var direction = controller.direction;
            animator.SetFloat(horizontalParameter, direction.x);
            animator.SetFloat(verticalParameter, direction.y);
        }
    }
}

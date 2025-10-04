using UnityEngine;

namespace Prototype
{
    using MoveMode = TopDownCharacterController2D.MoveMode;

    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Top-Down Character Controller (2D): Player")]
    public class TopDownCharacterControllerPlayer2D : MonoBehaviour
    {
        [SerializeField] string inputHorizontal = "Horizontal";
        [SerializeField] string inputVertical = "Vertical";

        public TopDownCharacterController2D controller { get; private set; }

        protected virtual void Awake()
        {
            controller = GetComponent<TopDownCharacterController2D>();
        }

        protected virtual void OnDisable()
        {
            if (controller)
                controller.move = new();
        }

        protected virtual void Update()
        {
            var moveMode = controller.moveMode;
            var raw = moveMode == MoveMode.RawPosition || moveMode == MoveMode.Acceleration;
            var input = new Vector2(
                raw ? Input.GetAxisRaw(inputHorizontal) : Input.GetAxis(inputHorizontal),
                raw ? Input.GetAxisRaw(inputVertical) : Input.GetAxis(inputVertical)
            );

            if (input.sqrMagnitude > 1)
                input.Normalize();

            controller.move = input;
        }
    }
}

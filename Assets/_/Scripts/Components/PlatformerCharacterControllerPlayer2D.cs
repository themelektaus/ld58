using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(PlatformerCharacterController2D))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Platformer Character Controller (2D): Player")]
    public class PlatformerCharacterControllerPlayer2D : MonoBehaviour
    {
        [SerializeField] string inputHorizontal = "Horizontal";
        [SerializeField] string inputJump = "Jump";

        [SerializeField] float inputJumpRemember = .1f;
        [SerializeField] bool holdToJump;

        PlatformerCharacterController2D controller;

        float jump;

        void Awake()
        {
            controller = GetComponent<PlatformerCharacterController2D>();
        }

        void Update()
        {
            controller.move = Input.GetAxisRaw(inputHorizontal);

            if (Input.GetButtonDown(inputJump) || (holdToJump && Input.GetButton(inputJump)))
                jump = Mathf.Max(Time.deltaTime, inputJumpRemember);

            controller.jump |= jump > 0;
            controller.jumpHolding = Input.GetButton(inputJump);

            jump = Mathf.Max(0, jump - Time.deltaTime);
        }
    }
}

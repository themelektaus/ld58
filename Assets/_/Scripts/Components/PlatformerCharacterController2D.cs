using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Platformer Character Controller (2D)")]
    public class PlatformerCharacterController2D : MonoBehaviour
    {
        [HideInInspector] public float move;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool jumpHolding;

        [ReadOnly] public Rigidbody2D body;

        [SerializeField, ReadOnly] PlatformerCharacterControllerGround2D ground;

        [SerializeField] AnimationUpdateMode updateMode = AnimationUpdateMode.Fixed;

        [SerializeField] float speed = 6;
        [SerializeField] float groundAcceleration = 45;
        [SerializeField] float airAcceleration = 45;
        [SerializeField] float jumpHeight = 8.2f;
        [SerializeField] float jumpHoldBreakSpeed = 20;
        [SerializeField] float jumpGravity = 2;
        [SerializeField] float fallGravity = 3;
        [SerializeField] float coyoteTime = .1f;
        [SerializeField] int jumpCount = 1;
        [SerializeField] Vector2 verticalVelocityRange = new(-15, 10);

        float moveVelocity;
        float airTime;
        float jumpTime;
        int currentJumpCount;

        void Awake()
        {
            OnValidate();
        }

        void OnValidate()
        {
            if (!body)
                body = GetComponentInChildren<Rigidbody2D>();

            if (!ground)
                ground = GetComponentInChildren<PlatformerCharacterControllerGround2D>();
        }

        void Update()
        {
            moveVelocity = move * Mathf.Max(speed - ground.friction, 0);
            jumpTime = Mathf.Max(0, jumpTime - Time.deltaTime);

            if (updateMode == AnimationUpdateMode.Normal)
                UpdateInternal(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (updateMode == AnimationUpdateMode.Fixed)
                UpdateInternal(Time.fixedDeltaTime);
        }

        void UpdateInternal(float deltaTime)
        {
            var velocity = body.linearVelocity;
            Update_Move(ref velocity, deltaTime);
            Update_Jump(ref velocity, deltaTime);
            Update_ClampVelocity(ref velocity);
            body.linearVelocity = velocity;

            Update_Reset();
        }

        void Update_Move(ref Vector2 velocity, float deltaTime)
        {
            float accelaration;
            if (ground.isTouching)
            {
                accelaration = groundAcceleration;
                airTime = 0;
            }
            else
            {
                accelaration = airAcceleration;
                airTime += deltaTime;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, moveVelocity, accelaration * deltaTime);
        }

        void Update_Jump(ref Vector2 velocity, float deltaTime)
        {
            if (airTime >= coyoteTime && currentJumpCount == 0)
                currentJumpCount = 1;

            if (jump)
            {
                if (jumpCount > 0 && (
                    ground.isTouching || (
                        jumpTime == 0 && (
                            airTime < coyoteTime ||
                            currentJumpCount < jumpCount
                        )
                    )
                ))
                {
                    jumpTime = .2f;
                    currentJumpCount++;

                    var jumpSpeed = Mathf.Sqrt(Physics2D.gravity.y * -jumpHeight);

                    if (velocity.y > 0)
                        jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0);

                    else if (velocity.y < 0)
                        jumpSpeed += Mathf.Abs(velocity.y);

                    velocity.y += jumpSpeed;
                }
            }

            jump = false;

            if (body.linearVelocity.y > 0)
            {
                body.gravityScale = jumpGravity;
                if (!ground.isTouching && !jumpHolding)
                    velocity.y -= jumpHoldBreakSpeed * deltaTime;
            }

            else if (body.linearVelocity.y < 0)
                body.gravityScale = fallGravity;

            else
                body.gravityScale = 1;
        }

        void Update_ClampVelocity(ref Vector2 velocity)
        {
            velocity.y = Utils.Clamp(velocity.y, verticalVelocityRange);
        }

        void Update_Reset()
        {
            if (!ground.isTouching)
                return;

            jumpTime = 0;
            currentJumpCount = 0;
        }

        public PlatformerCharacterControllerGround2D GetGround() => ground;
    }
}

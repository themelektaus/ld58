using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Platformer Character Controller (2D): Ground")]
    public class PlatformerCharacterControllerGround2D : MonoBehaviour
    {
        [HideInInspector] public bool isTouching;
        [HideInInspector] public float friction;

        void OnCollisionEnter2D(Collision2D collision) => OnCollision(collision, true);
        void OnCollisionStay2D(Collision2D collision) => OnCollision(collision, true);
        void OnCollisionExit2D(Collision2D collision) => OnCollision(null, false);

        void OnCollision(Collision2D collision, bool isTouching)
        {
            if (isTouching)
            {
                for (int i = 0; i < collision.contactCount; i++)
                {
                    var contact = collision.GetContact(i);
                    this.isTouching |= contact.normal.y >= 0.9f;
                }
            }
            else
            {
                this.isTouching = false;
            }

            friction = 0;

            if (collision is null)
                return;

            var body = collision.rigidbody;
            if (!body)
                return;

            var material = body.sharedMaterial;
            if (!material)
                return;

            friction = material.friction;
        }
    }
}

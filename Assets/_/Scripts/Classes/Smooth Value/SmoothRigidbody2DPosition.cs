using UnityEngine;

namespace Prototype
{
    public class SmoothRigidbody2DPosition : SmoothVector2
    {
        public readonly Rigidbody2D body;

        public float minDistance = 0;

        public SmoothRigidbody2DPosition(Rigidbody2D body, float smoothTime) : base(
            () => body.position,
            x => body.MovePosition(x),
            smoothTime
        )
        {
            this.body = body;
        }

        protected override Vector2 Update(Vector2 current, Vector2 target)
        {
            if (minDistance > 0 && (current - target).sqrMagnitude > minDistance * minDistance)
                return target;

            return base.Update(current, target);
        }
    }
}

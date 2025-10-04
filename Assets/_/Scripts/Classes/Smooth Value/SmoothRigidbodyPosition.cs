using UnityEngine;

namespace Prototype
{
    public class SmoothRigidbodyPosition : SmoothVector3
    {
        public readonly Rigidbody body;

        public float minDistance = 0;

        public SmoothRigidbodyPosition(Rigidbody body, float smoothTime) : base(
            () => body.position,
            x => body.MovePosition(x),
            smoothTime
        )
        {
            this.body = body;
        }

        protected override Vector3 Update(Vector3 current, Vector3 target)
        {
            if (minDistance > 0 && (current - target).sqrMagnitude > minDistance * minDistance)
                return target;

            return base.Update(current, target);
        }
    }
}

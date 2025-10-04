using UnityEngine;

namespace Prototype
{
    public class SmoothTransformPosition : SmoothVector3
    {
        public readonly Transform transform;

        public float minDistance = 0;

        public SmoothTransformPosition(Transform transform, float smoothTime) :
            base(
                () => transform.position,
                x => transform.position = x,
                smoothTime
            )
        {
            this.transform = transform;
        }

        protected override Vector3 Update(Vector3 current, Vector3 target)
        {
            if (minDistance > 0 && (current - target).sqrMagnitude > minDistance * minDistance)
                return target;

            return base.Update(current, target);
        }
    }
}

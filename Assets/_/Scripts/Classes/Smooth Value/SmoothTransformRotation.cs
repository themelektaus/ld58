using UnityEngine;

namespace Prototype
{
    public class SmoothTransformRotation : SmoothQuaternion
    {
        public readonly Transform transform;

        public SmoothTransformRotation(Transform transform, float smoothTime) : base(
            () => transform.rotation,
            x => transform.rotation = x,
            smoothTime
        )
        {
            this.transform = transform;
        }
    }
}

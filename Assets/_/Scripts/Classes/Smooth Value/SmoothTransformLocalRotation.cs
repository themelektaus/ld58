using UnityEngine;

namespace Prototype
{
    public class SmoothTransformLocalRotation : SmoothQuaternion
    {
        public readonly Transform transform;

        public SmoothTransformLocalRotation(Transform transform, float smoothTime) :
            base(
                () => transform.localRotation,
                x => transform.localRotation = x,
                smoothTime
            )
        {
            this.transform = transform;
        }
    }
}

using UnityEngine;

namespace Prototype
{
    public class SmoothTransformLocalScale : SmoothVector3
    {
        public readonly Transform transform;

        public SmoothTransformLocalScale(Transform transform, float smoothTime) :
            base(
                () => transform.localScale,
                x => transform.localScale = x,
                smoothTime
            )
        {
            this.transform = transform;
        }
    }
}

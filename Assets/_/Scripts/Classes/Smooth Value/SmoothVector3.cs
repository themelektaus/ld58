using System;

using UnityEngine;

namespace Prototype
{
    public class SmoothVector3 : SmoothValue<Vector3>
    {
        public float smoothTime;
        public float? maxSpeed;

        public Func<float> getDeltaTime = () => Time.deltaTime;

        protected Vector3 velocity;

        public SmoothVector3(Vector3 value, float smoothTime) : base(value)
        {
            this.smoothTime = smoothTime;
        }

        public SmoothVector3(Func<Vector3> getCurrent, Action<Vector3> setCurrent, float smoothTime) : base(getCurrent, setCurrent)
        {
            this.smoothTime = smoothTime;
        }

        protected override Vector3 Update(Vector3 current, Vector3 target)
        {
            if (smoothTime == 0)
                return target;

            if (maxSpeed.HasValue)
                return Vector3.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed.Value, getDeltaTime());

            return Vector3.SmoothDamp(current, target, ref velocity, smoothTime, float.PositiveInfinity, getDeltaTime());
        }
    }
}

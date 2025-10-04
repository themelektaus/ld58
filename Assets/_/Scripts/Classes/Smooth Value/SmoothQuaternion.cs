using System;

using UnityEngine;

namespace Prototype
{
    public class SmoothQuaternion : SmoothValue<Quaternion>
    {
        public float smoothTime;

        float x;
        float y;
        float z;

        public SmoothQuaternion(Quaternion value, float smoothTime) : base(value)
        {
            this.smoothTime = smoothTime;
        }

        public SmoothQuaternion(Func<Quaternion> getCurrent, Action<Quaternion> setCurrent, float smoothTime) : base(getCurrent, setCurrent)
        {
            this.smoothTime = smoothTime;
        }

        protected override Quaternion Update(Quaternion current, Quaternion target)
        {
            if (smoothTime == 0)
                return target;

            var a = current.eulerAngles;
            var b = target.eulerAngles;

            return Quaternion.Euler(
                Mathf.SmoothDampAngle(a.x, b.x, ref x, smoothTime),
                Mathf.SmoothDampAngle(a.y, b.y, ref y, smoothTime),
                Mathf.SmoothDampAngle(a.z, b.z, ref z, smoothTime)
            );
        }
    }
}

using System;

using UnityEngine;

namespace Prototype
{
    [Serializable]
    public class SmoothFloat : SmoothValue<float>
    {
        public float smoothTime;
        public float threshold = .0001f;

        public float boost;

        float velocity;
        bool isAngle;

        public SmoothFloat(float value, float smoothTime) : base(value)
        {
            this.smoothTime = smoothTime;
        }

        public SmoothFloat(Func<float> getCurrent, Action<float> setCurrent, float smoothTime) : base(getCurrent, setCurrent)
        {
            this.smoothTime = smoothTime;
        }

        public override void Update()
        {
            isAngle = false;
            base.Update();
        }

        public void Update(bool isAngle)
        {
            this.isAngle = isAngle;
            base.Update();
        }

        protected override float Update(float current, float target)
        {
            if (smoothTime == 0)
                return target;

            var result = isAngle ?
                Mathf.SmoothDampAngle(current, target, ref velocity, smoothTime) :
                Mathf.SmoothDamp(current, target, ref velocity, smoothTime);

            if (boost > 0)
            {
                if (result < target)
                    result = Mathf.Min(result + boost * Time.deltaTime, target);
                else if (result > target)
                    result = Mathf.Max(result - boost * Time.deltaTime, target);
            }

            if (threshold > 0 && Utils.Approximately(result, target, threshold))
            {
                velocity = 0;
                result = target;
            }

            return result;
        }
    }
}

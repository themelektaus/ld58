using System;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_OVERRIDES + "Time Scale")]
    public class TimeScaleOverride : OverrideBehaviour<Type, float>
    {
        [SerializeField, Range(0, 2)] float timeScale = 1;

        protected override Type GetTarget()
        {
            return typeof(TimeScaleOverride);
        }

        protected override void Setup(out float originalValue)
        {
            originalValue = Time.timeScale;
        }

        protected override void Process(ref float value, float weight)
        {
            value = Mathf.Lerp(value, timeScale, weight);
        }

        protected override void Apply(in float value)
        {
            Time.timeScale = value;
        }
    }
}

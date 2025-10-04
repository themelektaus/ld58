using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_OVERRIDES + "Blur")]
    public class BlurOverride : OverrideBehaviour<Material, float>
    {
        protected override bool useLateUpdate => true;
        protected override bool useMaxWeight => true;

        [SerializeField] Material material;

        [SerializeField] bool overrideMultiplyTintColor;
        [SerializeField] Color multiplyTintColor = Color.white;

        [SerializeField] bool overrideAdditiveTintColor;
        [SerializeField] Color additiveTintColor = new(0, 0, 0, 0);

        float target;

        static readonly SmoothFloat smoothTarget = new(0, .1f);

        protected override void OnAwake()
        {
            target = 1;
            material.SetFloat("_Weight", 0);
            material.SetFloat("_Size", 4);
        }

        protected override Material GetTarget()
        {
            return material;
        }

        protected override void Setup(out float originalValue)
        {
            originalValue = 0;
        }

        protected override void Process(ref float value, float weight)
        {
            value = Mathf.Lerp(value, target, weight);
        }

        protected override void Apply(in float value)
        {
            smoothTarget.target = value;
            smoothTarget.Update();

            material.SetFloat("_Weight", smoothTarget);
            material.SetFloat("_Size", Screen.width / 1920f * 4);

            if (overrideMultiplyTintColor && smoothTarget > 0)
                material.SetColor("_MultiplyColor", multiplyTintColor);

            if (overrideAdditiveTintColor && smoothTarget > 0)
                material.SetColor("_AdditiveColor", additiveTintColor);
        }
    }
}

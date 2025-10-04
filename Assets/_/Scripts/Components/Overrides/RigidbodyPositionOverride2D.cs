using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_OVERRIDES + "Rigidbody Position (2D)")]
    public class RigidbodyPositionOverride2D : OverrideBehaviour<Rigidbody2D, Vector2>
    {
        [SerializeField] Rigidbody2D body;

        Vector2 target;

        protected override void OnAwake()
        {
            target = transform.position;
        }

        protected override Rigidbody2D GetTarget()
        {
            return body;
        }

        protected override void Setup(out Vector2 originalValue)
        {
            originalValue = body.position;
        }

        protected override void Process(ref Vector2 value, float weight)
        {
            value = Vector2.Lerp(value, target, weight);
        }

        protected override void Apply(in Vector2 value)
        {
            body.MovePosition(value);
        }
    }
}

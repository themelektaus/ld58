using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Trigger (2D)")]
    public class Trigger2DCondition : Condition
    {
        [SerializeField, FormerlySerializedAs("query")] Object @object;

        [SerializeField] bool stayTrue;

        public bool wasTrue;

        public readonly HashSet<Collider2D> colliders = new();

        void OnEnable() => Update();

        protected override void OnDisable()
        {
            colliders.Clear();

            if (!stayTrue)
                base.OnDisable();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (@object && !@object.Match(collision))
                return;

            colliders.Add(collision);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (@object && !@object.Match(collision))
                return;

            colliders.Remove(collision);
        }

        public override IEnumerable<bool> If()
        {
            var @true = colliders.Count > 0;
            yield return (stayTrue && (wasTrue |= @true)) || @true;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Overlap Point (2D)")]
    public class OverlapPoint2DCondition : Condition
    {
        [SerializeField] Reference[] pointReferences;

#if UNITY_EDITOR
        new Collider2D collider;
#else
        Collider2D collider;
#endif

        void Awake()
        {
            collider = GetComponent<Collider2D>();
        }

        public override IEnumerable<bool> If()
        {
            return pointReferences.Select(x => collider.OverlapPoint(x.Get<Vector2>()));
        }
    }
}

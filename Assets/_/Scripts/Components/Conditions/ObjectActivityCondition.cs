using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Object Activity")]
    public class ObjectActivityCondition : Condition
    {
        public enum Scope { Self, Hierarchy }

        [SerializeField] Scope scope;
        [SerializeField] Object[] objects;

        public override IEnumerable<bool> If()
        {
            return objects.Select(x => x.IsActiveOrEnabled(scope == Scope.Self));
        }
    }
}

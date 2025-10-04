using System.Collections.Generic;

using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Is Button Down")]
    public class IsButtonDownCondition : Condition
    {
        [SerializeField] string[] buttons;

        protected override void OnDisable() { }

        public override IEnumerable<bool> If()
        {
            return buttons.Select(x => Input.GetButtonDown(x) || Input.GetButton(x));
        }
    }
}

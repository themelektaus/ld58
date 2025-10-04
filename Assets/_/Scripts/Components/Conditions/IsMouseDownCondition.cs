using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Is Mouse Down")]
    public class IsMouseDownCondition : Condition
    {
        [SerializeField] bool leftMouseButton;
        [SerializeField] bool rightMouseButton;
        [SerializeField] bool middleMouseButton;

        protected override void OnDisable() { }

        public override IEnumerable<bool> If()
        {
            if (leftMouseButton)
                yield return Input.GetMouseButton(0) || Input.GetMouseButtonDown(0);

            if (rightMouseButton)
                yield return Input.GetMouseButton(1) || Input.GetMouseButtonDown(1);

            if (middleMouseButton)
                yield return Input.GetMouseButton(2) || Input.GetMouseButtonDown(2);
        }
    }
}

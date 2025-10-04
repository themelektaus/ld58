using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Disable")]
    public class OnDisable_ : On
    {
        protected override bool offloadCoroutine => true;

        void OnDisable()
        {
            Invoke();
        }
    }
}

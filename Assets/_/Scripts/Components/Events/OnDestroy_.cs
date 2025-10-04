using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Destroy")]
    public class OnDestroy_ : On
    {
        protected override bool offloadCoroutine => true;

        void OnDestroy()
        {
            Invoke();
        }
    }
}

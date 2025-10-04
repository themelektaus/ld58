using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Awake")]
    public class OnAwake : On
    {
        protected override bool offloadCoroutine => true;

        void Awake()
        {
            Invoke();
        }
    }
}

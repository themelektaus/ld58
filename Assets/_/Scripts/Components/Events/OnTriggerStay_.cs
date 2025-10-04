using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Stay")]
    public class OnTriggerStay_ : On<Collider>
    {
        void OnTriggerStay(Collider collider)
        {
            if (enabled)
            {
                Invoke(collider);
            }
        }
    }
}

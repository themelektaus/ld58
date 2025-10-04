using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Exit")]
    public class OnTriggerExit_ : On<Collider>
    {
        void OnTriggerExit(Collider collider)
        {
            if (enabled)
            {
                Invoke(collider);
            }
        }
    }
}

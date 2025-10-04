using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Enter")]
    public class OnTriggerEnter_ : On<Collider>
    {
        void OnTriggerEnter(Collider collider)
        {
            if (enabled)
            {
                Invoke(collider);
            }
        }
    }
}

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Collision Exit")]
    public class OnCollisionExit_ : On<Collision>
    {
        void OnCollisionExit(Collision collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

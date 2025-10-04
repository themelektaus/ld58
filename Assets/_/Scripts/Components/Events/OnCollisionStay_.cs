using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Collision Stay")]
    public class OnCollisionStay_ : On<Collision>
    {
        void OnCollisionStay(Collision collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

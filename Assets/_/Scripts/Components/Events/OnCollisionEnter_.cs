using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Collision Enter")]
    public class OnCollisionEnter_ : On<Collision>
    {
        void OnCollisionEnter(Collision collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

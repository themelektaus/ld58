using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Stay (2D)")]
    public class OnTriggerStay2D_ : On<Collider2D>
    {
        void OnTriggerStay2D(Collider2D collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

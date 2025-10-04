using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Exit (2D)")]
    public class OnTriggerExit2D_ : On<Collider2D>
    {
        void OnTriggerExit2D(Collider2D collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

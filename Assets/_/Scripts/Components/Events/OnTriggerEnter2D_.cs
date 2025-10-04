using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Trigger Enter (2D)")]
    public class OnTriggerEnter2D_ : On<Collider2D>
    {
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Collision Enter 2D")]
    public class OnCollisionEnter2D_ : On<Collision2D>
    {
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled)
            {
                Invoke(collision);
            }
        }
    }
}

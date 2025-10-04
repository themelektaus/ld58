using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Start")]
    public class OnStart : On
    {
        void Start()
        {
            Invoke();
        }
    }
}

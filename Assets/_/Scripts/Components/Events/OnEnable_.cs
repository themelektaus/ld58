using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Enable")]
    public class OnEnable_ : On
    {
        void OnEnable()
        {
            Invoke();
        }
    }
}

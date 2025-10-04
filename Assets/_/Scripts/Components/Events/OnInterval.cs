using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Interval")]
    public class OnInterval : On
    {
        [SerializeField] float interval = 1;

        readonly Timer timer = new();

        void Update()
        {
            if (timer.Update(interval))
                Invoke();
        }
    }
}

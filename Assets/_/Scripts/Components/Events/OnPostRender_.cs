using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Post Render")]
    public class OnPostRender_ : On
    {
        void OnPostRender()
        {
            if (!enabled)
            {
                this.LogWarning($"{nameof(OnPostRender_)} not enabled");
            }

            Invoke();
        }
    }
}

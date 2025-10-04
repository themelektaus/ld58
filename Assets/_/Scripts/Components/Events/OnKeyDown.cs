using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Key Down")]
    public class OnKeyDown : On
    {
        [SerializeField] KeyCode key = KeyCode.Space;

        void Update()
        {
            if (Input.GetKeyDown(key))
            {
                Invoke();
            }
        }
    }
}

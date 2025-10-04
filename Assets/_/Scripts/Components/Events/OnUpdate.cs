using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Update")]
    public class OnUpdate : On<float>
    {
        [SerializeField] float deltaTimeMultiplier = 1;

        void Update()
        {
            Invoke(deltaTimeMultiplier * Time.deltaTime);
        }
    }
}

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Deactivate")]
    public class Deactivate : MonoBehaviour
    {
        public float delay;

        Sequence.Instance sequence;

        void Awake()
        {
            if (delay == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            sequence = this
                .Wait(delay)
                .Deactivate(gameObject)
                .Build();
        }

        void OnEnable()
        {
            sequence?.Start();
        }

        void OnDisable()
        {
            sequence?.Stop();
        }
    }
}

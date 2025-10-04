using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Destroy")]
    public class Destroy : MonoBehaviour
    {
        public float delay;
        public bool delayByFrameCount;

        Sequence.Instance sequence;

        void Awake()
        {
            if (delay > 0)
            {
                sequence = (
                    delayByFrameCount
                        ? this.WaitForFrames(Mathf.CeilToInt(delay))
                        : this.Wait(delay)
                )
                    .Destroy(gameObject)
                    .Build();
            }
        }

        void OnEnable()
        {
            if (sequence is null)
            {
                gameObject.Destroy();
            }
            else
            {
                sequence.Start();
            }
        }

        void OnDisable()
        {
            if (sequence is not null)
            {
                sequence.Stop();
            }
        }
    }
}

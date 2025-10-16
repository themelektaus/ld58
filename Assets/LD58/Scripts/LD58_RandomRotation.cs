using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_RandomRotation : MonoBehaviour
    {
        [SerializeField] Vector2 range = new(0, 360);

        void Awake()
        {
            var eulerAngles = transform.localEulerAngles;
            eulerAngles.z = Mathf.Lerp(range.x, range.y, Utils.RandomFloat());
            transform.localEulerAngles = eulerAngles;
        }
    }
}

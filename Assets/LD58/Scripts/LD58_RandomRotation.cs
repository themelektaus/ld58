using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_RandomRotation : MonoBehaviour
    {
        void Awake()
        {
            var eulerAngles = transform.localEulerAngles;
            eulerAngles.z = Utils.RandomFloat() * 360;
            transform.localEulerAngles = eulerAngles;
        }
    }
}

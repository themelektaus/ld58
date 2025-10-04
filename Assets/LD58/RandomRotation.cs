using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    void Awake()
    {
        var eulerAngles = transform.localEulerAngles;
        eulerAngles.z = Prototype.Utils.RandomFloat() * 360;
        transform.localEulerAngles = eulerAngles;
    }
}

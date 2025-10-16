using UnityEngine;

public class LD58_ResetPosition : MonoBehaviour
{
    void Awake()
    {
        transform.localPosition = default;
    }
}

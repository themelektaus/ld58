using Prototype;
using UnityEngine;

public class SpawnStopper : MonoBehaviour
{
    [SerializeField] int threshold = 8;
    readonly Prototype.Timer timer = new();

    void Update()
    {
        if (timer.Update(1))
        {
            if (FindObjectsByType<TrashObject>(FindObjectsSortMode.None).Length > threshold)
                return;

            gameObject.Destroy();
        }
    }
}

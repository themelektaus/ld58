using UnityEngine;

namespace Prototype
{
    [AddComponentMenu("/")]
    public class Dummy : MonoBehaviour
    {
        public static Dummy Create()
            => new GameObject().AddComponent<Dummy>();

        public static Dummy Create(string name)
            => new GameObject(name).AddComponent<Dummy>();
    }
}

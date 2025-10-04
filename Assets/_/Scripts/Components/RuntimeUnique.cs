using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Runtime Unique")]
    public class RuntimeUnique : MonoBehaviour, IUnique
    {
        public int id { get; set; }
        public int GetId() => id;
    }
}

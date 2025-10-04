using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Object Context")]
    public class ObjectContext : MonoBehaviour
    {
        public Object @object;

        public new T GetComponent<T>() where T : Component
        {
            return ExtensionMethods.GetComponent<T>(this);
        }
    }
}

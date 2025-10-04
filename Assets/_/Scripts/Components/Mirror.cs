using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Mirror")]
    public class Mirror : MonoBehaviour
    {
        [SerializeField] bool updateInEditor;
        [SerializeField] Reference source;
        [SerializeField] Reference target;

        void Update()
        {
            if (!updateInEditor && !Application.isPlaying)
                return;

            if (source is null || source.isEmpty)
                return;

            if (target is null || target.isEmpty)
                return;

            target.Set(source.Get());
        }
    }
}

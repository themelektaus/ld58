#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Default Parent (Editor)")]
    public class DefaultParentForEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        static DefaultParentForEditor selection;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() => selection = null;

        void Update()
        {
            if (Application.isPlaying)
                return;

            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != gameObject)
                return;

            if (!activeGameObject.TryGetComponent(out DefaultParentForEditor defaultParent))
                return;

            if (selection && selection != defaultParent)
            {
                selection = null;
                EditorUtility.ClearDefaultParentObject();
            }

            if (!selection)
            {
                selection = this;
                EditorUtility.SetDefaultParentObject(gameObject);
            }
        }
#endif
    }
}

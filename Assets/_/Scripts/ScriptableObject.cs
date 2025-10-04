#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Prototype
{
    public abstract class ScriptableObject : UnityEngine.ScriptableObject
    {
        protected abstract void Initialize();

#if UNITY_EDITOR
        void OnEnable()
        {
            Initialize();
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                Initialize();
                return;
            }

            if (change == PlayModeStateChange.ExitingEditMode)
            {
                Initialize();
                return;
            }
        }
#endif
    }
}

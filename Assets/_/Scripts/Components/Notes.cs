using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Notes")]
    public class Notes : MonoBehaviour
    {
        public string text;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            this.DrawEditorLabel(name, text, new() { offset = new(0, .25f) });
        }
#endif
    }
}

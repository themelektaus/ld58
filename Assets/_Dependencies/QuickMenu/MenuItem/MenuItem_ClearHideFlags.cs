using UnityEngine;

namespace QuickMenu
{
    public class MenuItem_ClearHideFlags : MenuItem
    {
        public override string title => "Clear Hide Flags";

        public override bool Command(Context context)
        {
            foreach (var gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                gameObject.hideFlags = HideFlags.None;
                foreach (var component in gameObject.GetComponents(typeof(Component)))
                    component.hideFlags = HideFlags.None;
            }
            return true;
        }
    }
}

using UnityEditor;

namespace QuickMenu
{
    public class MenuItem_SelectParent : MenuItem
    {
        public override string title => "Select Parent";

        public override bool Validation(Context context)
        {
            if (!context.transform || !context.transform.parent)
                return false;

            return true;
        }

        public override bool Command(Context context)
        {
            Selection.activeObject = context.transform.parent.gameObject;
            return true;
        }
    }
}
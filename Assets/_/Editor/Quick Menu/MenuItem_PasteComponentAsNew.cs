using QuickMenu;

using UnityEditorInternal;
using UnityEngine;

namespace Prototype.Editor
{
    public class MenuItem_PasteComponentAsNew : MenuItem
    {
        public override string title => "Paste Component As New";

        [UnityEditor.MenuItem("Edit/Paste Component As New")]
        public static bool PasteComponentAsNew()
        {
            return PasteComponentAsNew(UnityEditor.Selection.activeGameObject);
        }

        public override bool Command(Context context)
        {
            return PasteComponentAsNew(context.gameObject);
        }

        static bool PasteComponentAsNew(GameObject gameObject)
        {
            if (!gameObject)
                return false;

            if (!ComponentUtility.PasteComponentAsNew(gameObject))
                return false;

            return true;
        }
    }
}

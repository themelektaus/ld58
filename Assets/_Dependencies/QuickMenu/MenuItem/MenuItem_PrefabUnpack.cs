using UnityEditor;

namespace QuickMenu
{
    public class MenuItem_PrefabUnpack : MenuItem
    {
        public override string category => "Prefab";

        public override string title => "Unpack";

        public bool completely = false;

        public override bool Validation(Context context)
        {
            if (!context.gameObject)
                return false;

            if (!PrefabUtility.GetCorrespondingObjectFromSource(context.gameObject))
                return false;

            return true;
        }

        public override bool Command(Context context)
        {
            PrefabUtility.UnpackPrefabInstance(
                context.gameObject,
                completely ? PrefabUnpackMode.Completely : PrefabUnpackMode.OutermostRoot,
                InteractionMode.UserAction
            );
            return true;
        }
    }
}
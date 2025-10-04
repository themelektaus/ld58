using QuickMenu;

using UnityEngine;

namespace Prototype.Editor
{
    public class MenuItem_DebugLog : MenuItem
    {
        public override string title => "Log";
        public override string description => "Writes a Message into the Console";

        public override string category => "Debug";

        public string message;

        public override bool visible => false;

        public override bool Command(Context context)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            Debug.Log(message);
            return true;
        }
    }
}

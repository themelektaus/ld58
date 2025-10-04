using QuickMenu;

using System.Linq;

namespace Prototype.Editor
{
    public class MenuItem_ClearConsole : MenuItem
    {
        public override string title => "Clear Console";

        public override string category => "Debug";

        public override bool Command(Context context)
        {
            Utils.types.FirstOrDefault(x => x.FullName == "UnityEditor.LogEntries")
                       .GetMethod("Clear")
                       .Invoke(null, null);
            return true;
        }
    }
}

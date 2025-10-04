using QuickMenu;

namespace Prototype.Editor
{
    public class MenuItem_Recompile : MenuItem
    {
        public override string title => "Recompile";

        public override string category => "Tool";

        public override bool Command(Context context)
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            return true;
        }
    }
}

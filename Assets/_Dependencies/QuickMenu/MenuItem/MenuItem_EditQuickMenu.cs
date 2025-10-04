namespace QuickMenu
{
    internal class MenuItem_EditQuickMenu : MenuItem
    {
        public override string title => "Edit Quick Menu Executions";

        public override bool Validation(Context context)
        {
            if (context.isSceneHierarchy)
                return false;

            return base.Validation(context);
        }

        public override bool Command(Context context)
        {
            QuickMenuExecutionsWindow.Open();
            return true;
        }
    }
}
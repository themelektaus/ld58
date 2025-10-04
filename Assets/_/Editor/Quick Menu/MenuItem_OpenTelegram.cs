using QuickMenu;

using System;
using System.Diagnostics;
using System.IO;

namespace Prototype.Editor
{
    public class MenuItem_OpenTelegram : MenuItem
    {
        public override string title => "Open Telegram";
        public override string description => "Opens the installed Telegram Client on your PC";

        public override string category => "Special";

        public override bool visible => false;

        public override bool Command(Context context)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Process.Start(Path.Combine(appdata, @"Telegram Desktop\Telegram.exe"));
            return true;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;

namespace QuickMenu
{
	public class MenuItem_ScreenshotGameView : MenuItem
	{
		public override string title => "Screenshot (Game View)";
		public override string description => "Takes a screenshot from current Game View";

		public override string category => "Tool";

		public override bool visible => false;

		public override bool Command(Context context)
		{
			var now = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			var filename = Path.Combine("Recordings", $"Screenshot_{now}.png");
			if (File.Exists(filename))
			{
				UnityEngine.Debug.LogError($"File already exists. (\"{filename}\")");
				return false;
			}

			Directory.CreateDirectory("Recordings");

			ScreenCapture.CaptureScreenshot(filename);

			EditorApplication.ExecuteMenuItem("Window/General/Game");

			Task.Run(async () =>
			{
				int i = 0;
				while (i++ < 30 && !File.Exists(filename))
					await Task.Delay(200);

				if (File.Exists(filename))
				{
					UnityEngine.Debug.Log($"Screenshot successfully saved. (\"{filename}\")");
					Process.Start("mspaint", $"\"{filename.Replace('/', '\\')}\"");
					return;
				}

				UnityEngine.Debug.LogError($"Can not save screenshot. (\"{filename}\")");
			});
			return true;
		}
	}
}

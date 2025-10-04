using UnityEditor.SceneManagement;

using UnityEngine.SceneManagement;

namespace QuickMenu
{
	public class MenuItem_SceneDiscardChanges : MenuItem
	{
		public override int priority => 1;

		public override string category => "Scene";

		public override string title => "Discard Changes";

		public override bool Validation(Context context)
		{
			return context.scene.name is not null;
		}

		public override bool Command(Context context)
		{
			if (SceneManager.sceneCount == 1)
			{
				EditorSceneManager.OpenScene(context.scene.path);
				return true;
			}

			bool wasActive = SceneManager.GetActiveScene() == context.scene;

			SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
			Scene? sceneBelow = null;
			for (int i = 0; i < setup.Length; i++)
			{
				if (setup[i].path == context.scene.path &&
					i < setup.Length - 1)
				{
					sceneBelow = SceneManager.GetSceneAt(i + 1);
					break;
				}
			}

			var path = context.scene.path;
			EditorSceneManager.CloseScene(context.scene, true);
			Scene reopenedScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

			if (wasActive) SceneManager.SetActiveScene(reopenedScene);
			if (sceneBelow.HasValue)
				EditorSceneManager.MoveSceneBefore(reopenedScene, sceneBelow.Value);

			return true;
		}
	}
}

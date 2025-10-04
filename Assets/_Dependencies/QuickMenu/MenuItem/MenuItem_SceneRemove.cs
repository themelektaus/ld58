using UnityEditor.SceneManagement;

namespace QuickMenu
{
    public class MenuItem_SceneRemove : MenuItem
    {
        public override int priority => 1;

        public override string category => "Scene";

        public override string title => "Remove";

        public override bool Validation(Context context)
        {
            return context.scene.name is not null;
        }

        public override bool Command(Context context)
        {
            foreach (var scene in context.scenes)
            {
                EditorSceneManager.CloseScene(scene, true);
                if (context.scenes.Count <= 1)
                    break;
            }
            return true;
        }
    }
}
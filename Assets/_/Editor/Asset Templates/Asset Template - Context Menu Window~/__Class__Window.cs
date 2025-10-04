using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace __Namespace__
{
    public class __Class__Window : EditorWindow
    {
        [SerializeField]
        VisualTreeAsset visualTreeAsset;

        [MenuItem("Assets/__Class__")]
        public static void Open()
        {
            var window = GetWindow<__Class__Window>("__Class__");
            window.Refresh();
        }

        [MenuItem("Assets/__Class__", validate = true)]
        public static bool Open_Validate()
        {
            return true;
        }

        VisualElement content;

        public void CreateGUI()
        {
            if (!visualTreeAsset)
                return;

            VisualElement root = rootVisualElement;

            root.AddVisualTreeAsset(visualTreeAsset);

            content = new();

            root.Add(content);
        }

        void Refresh()
        {
            content.Clear();

            //add some visual elements here

        }
    }
}

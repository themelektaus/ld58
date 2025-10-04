using UnityEditor;

using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class ScenePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.ToSceneField(property);
            return root;
        }
    }
}

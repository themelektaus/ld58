using UnityEditor;

using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomPropertyDrawer(typeof(TypeAttribute))]
    public class TypePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.ToTypeField(property);
            return root;
        }
    }
}

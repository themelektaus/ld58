using UnityEditor;

using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomEditor(typeof(ObjectQuery))]
    public class ObjectQueryEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return serializedObject.CreateDefaultEditor();
        }
    }
}

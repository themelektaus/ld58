using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    [CustomEditor(typeof(Spawner))]
    public class SpawnerEditor : UnityEditor.Editor
    {
        VisualElement objects;

        public override VisualElement CreateInspectorGUI()
        {
            var root = this.CreateRootElement(new(0, .7f, 0, .05f));

            root.AddVisualTreeAsset<SpawnerEditor>();

            objects = root.Q<VisualElement>("Objects");

            objects.ToListElement(

                itemVisualTreeAssetName:
                    "SpawnerObjectEditor",

                property:
                    serializedObject.FindProperty("objects"),

                filter:
                    x => x.Filter<GameObject>().ToArray(),

                addAction:
                    (item, gameObject) =>
                    {
                        item.FindPropertyRelative("gameObject").objectReferenceValue = gameObject;
                        item.FindPropertyRelative("lifetime").floatValue = 0;
                    }
            );

            root.Bind(serializedObject);

            return root;
        }
    }
}

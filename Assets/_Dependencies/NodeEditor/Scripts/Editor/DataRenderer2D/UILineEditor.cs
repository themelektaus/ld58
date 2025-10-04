using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace NodeEditor.DataRenderer2D.Editors
{
    [CustomEditor(typeof(UILine))]
    public class UILineEditor : ImageEditor
    {
        PointHandler _pointHandler;
        MonoBehaviour _owner;

        SerializedProperty _SplineData;

        protected override void OnEnable()
        {
            base.OnEnable();
            _owner = target as MonoBehaviour;
            _SplineData = serializedObject.FindProperty("line");

            _pointHandler = new PointHandler(_owner, serializedObject);
        }

        void OnSceneGUI()
        {
            _pointHandler.OnSceneGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_SplineData, true);

            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace NodeEditor.DataRenderer2D.Editors
{
    public class LineEditor : Editor
    {
        PointHandler _pointHandler;
        MonoBehaviour _owner;

        protected void OnEnable()
        {
            _owner = target as MonoBehaviour;
            _pointHandler = new PointHandler(_owner, serializedObject);
        }

        protected void OnSceneGUI()
        {
            _pointHandler.OnSceneGUI();
        }
    }
}
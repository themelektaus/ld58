using UnityEditor;
using UnityEngine;

namespace NodeEditor.DataRenderer2D.Editors
{
    public static class MenuExtender
    {
        [MenuItem("GameObject/2D Object/UI Line")]
        public static void CreateUILine()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();

            if (!canvas)
            {
                var canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var linego = new GameObject("line");
            var uiline = linego.AddComponent<UILine>();
            uiline.line.Initialize();

            uiline.transform.SetParent(canvas.transform);
            uiline.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            Selection.activeObject = linego;
        }
    }
}
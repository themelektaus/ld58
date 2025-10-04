using UnityEngine;

namespace NodeEditor
{
    public class Viewport : MonoBehaviour
    {
        public float zoom = 1;
        float zoomTarget = 1;
        float zoomVelocity;
        float zoomSmoothTime = .05f;

        [HideInInspector] public bool zoomEnabled;

        bool pan;
        Vector2 panOrigin;
        Vector2 panMousePosition;

        Canvas canvas;
        NodeEditor nodeEditor;
        
        void Awake()
        {
            zoomEnabled = true;
            canvas = FindFirstObjectByType<Canvas>();
            nodeEditor = FindFirstObjectByType<NodeEditor>();
        }

        public void FrameAll()
        {
            zoom = 1;
            zoomTarget = 1;
            zoomVelocity = 0;

            var average = Vector2.zero;
            foreach (var nodeData in nodeEditor.graphData.nodeDataList)
                average += nodeData.position;


            transform.localPosition = Utils.Approximately(average, Vector2.zero) ? Vector2.zero : -average / nodeEditor.graphData.nodeDataList.Count;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                FrameAll();

            var oldZoomCurrent = zoom;
            var oldZoomTarget = zoomTarget;

            if (zoomEnabled)
                zoomTarget = Mathf.Clamp(oldZoomTarget + oldZoomTarget * (Input.mouseScrollDelta.y / 10), .01f, 10);

            zoom = Mathf.SmoothDamp(zoom, zoomTarget, ref zoomVelocity, zoomSmoothTime);
            
            transform.localScale = Vector3.one * zoom;

            if (oldZoomCurrent != zoom)
            {
                var position = transform.localPosition;
                var zoomDelta = oldZoomCurrent - zoom;
                position.x -= position.x / zoom * zoomDelta;
                position.y -= position.y / zoom * zoomDelta;
                transform.localPosition = position;
            }
            
            var mousePosition = Input.mousePosition / canvas.scaleFactor;
            
            if (Input.GetMouseButtonDown(2))
            {
                pan = true;
                panOrigin = transform.localPosition;
                panMousePosition = mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
                pan = false;

            if (pan)
                transform.localPosition = panOrigin + (Vector2) mousePosition - panMousePosition;
        }
    }
}

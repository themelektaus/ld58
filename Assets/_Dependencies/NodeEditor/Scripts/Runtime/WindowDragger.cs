using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NodeEditor
{
    public class WindowDragger : UIBehaviour, IBeginDragHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform dragObject;

        RectTransform root;
        Viewport viewport;

        Vector2 orignalPoint;
        Vector3 orignalDragObjectPosition;

        static WindowDragger owner;
        readonly static List<Node> otherNodes = new();

        protected override void Awake()
        {
            root = transform.root as RectTransform;
            viewport = GetComponentInParent<Viewport>();
        }

        Vector3 GetPosition(PointerEventData data)
        {
            return data.position / viewport.zoom;
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button != 0)
                return;

            orignalDragObjectPosition = dragObject.localPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, GetPosition(data), data.pressEventCamera, out orignalPoint);
            transform.SetAsLastSibling();
            dragObject.transform.SetAsLastSibling();

            var nodeEditor = GetComponentInParent<NodeEditor>();
            if (owner)
                return;

            owner = this;
            otherNodes.Clear();

            if (!nodeEditor.GetSelectedNodes().Contains(GetComponentInParent<Node>()))
                return;

            foreach (var node in nodeEditor.GetSelectedNodes())
            {
                if (node.dragger != this)
                {
                    node.dragger.OnBeginDrag(data);
                    otherNodes.Add(node);
                }
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (data.button != 0)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, GetPosition(data), data.pressEventCamera, out var localPoint))
                return;

            Vector3 offsetToOriginal = localPoint - orignalPoint;
            dragObject.localPosition = orignalDragObjectPosition + offsetToOriginal;

            if (owner != this)
                return;
            
            foreach (var node in otherNodes)
                node.dragger.OnDrag(data);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            owner = null;
            otherNodes.Clear();
        }
    }
}
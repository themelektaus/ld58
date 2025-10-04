using System;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    using DataRenderer2D;

    [RequireComponent(typeof(UILine))]
    public class Connection : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public long sourceNodeDataId;
            public long targetNodeDataId;

            [NonSerialized] public Node.Data sourceNodeData;
            [NonSerialized] public Node.Data targetNodeData;
            
            [NonSerialized] public NodeEditor nodeEditor;
            [NonSerialized] public Connection connection;
        }

        Data data;
        
        [SerializeField] GameObject bounds;
        [Range(.5f, 20), SerializeField] float width = 5;
        [Range(0, 1), SerializeField] float curve = .5f;
        
        UILine uiLine;
        Vector3? sourcePoint;
        Vector3? targetPoint;
        
        void Awake()
        {
            uiLine = GetComponent<UILine>();
        }

        void Update()
        {
            if (!uiLine)
                return;
            
            Vector3 sourcePoint = data.sourceNodeData.node.outPoint;
            
            Vector3 targetPoint;
            if (data.targetNodeData is null)
            {
                bounds.SetActive(false);
                targetPoint = Input.mousePosition;
            }
            else
            {
                bounds.SetActive(true);
                targetPoint = data.targetNodeData.node.inPoint;
            }

            if (this.sourcePoint.HasValue && this.targetPoint.HasValue)
                if (Utils.Approximately(this.sourcePoint.Value, sourcePoint))
                    if (Utils.Approximately(this.targetPoint.Value, targetPoint))
                        return;
            
            this.sourcePoint = sourcePoint;
            this.targetPoint = targetPoint;

            var offset = targetPoint - sourcePoint;
            var curve = offset.magnitude * this.curve / transform.lossyScale.x;
            
            uiLine.line.EditPoint(0, sourcePoint, new(curve, 0, 10), new(), width);
            uiLine.line.EditPoint(1, targetPoint, new(), new(-curve, 0, 10), width);
        }

        public void Read(Data data)
        {
            this.data = data;
            var nodeDataList = data.nodeEditor.graphData.nodeDataList;
            data.sourceNodeData = nodeDataList.FirstOrDefault(x => x.id == data.sourceNodeDataId);
            data.targetNodeData = nodeDataList.FirstOrDefault(x => x.id == data.targetNodeDataId);
            Update();
        }

        public void Remove()
        {
            data.nodeEditor.RemoveConnection(data);
        }
    }
}
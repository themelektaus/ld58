using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    public class NodeEditor : MonoBehaviour
    {
        [SerializeField] GameObject viewport;
        [SerializeField] GameObject graph;
        [SerializeField] GameObject cancelArea;

        [SerializeField] GameObject openGraphDialogPrefab;

        [SerializeField] Connection connection;
        [SerializeField] RectTransform connectionParent;

        [SerializeField] List<Node> nodes;
        [SerializeField] RectTransform nodeParent;

        [SerializeField] ContextMenuContent createMenu;
        [SerializeField] Sprite addIcon;
        [SerializeField] Sprite closeGraphIcon;

        [SerializeField] RectTransform selection;

        [SerializeField] UnityEngine.Events.UnityEvent onQuitGraph;

        public GraphData graphData { get; private set; }

        Canvas canvas;
        GameObject openGraphDialog;

        Connection.Data pendingConnectionData;

        Vector3 selectionStart;
        
        readonly Stack<string> guidStack = new();
        
        float updateTimer;

        void Awake()
        {
            canvas = FindFirstObjectByType<Canvas>();

            ContextMenuContent.ContextItem item;

            foreach (var node in nodes)
            {
                if (!node)
                {
                    item = new() { contextItemType = ContextMenuContent.ContextItemType.Separator };
                    createMenu.items.Add(item);
                    continue;
                }

                item = new()
                {
                    contextItemType = ContextMenuContent.ContextItemType.Button,
                    itemIcon = addIcon,
                    itemText = node.type,
                    onClick = new()
                };
                item.onClick.AddListener(() => AddNode(node.type));
                createMenu.items.Add(item);
            }

            item = new() { contextItemType = ContextMenuContent.ContextItemType.Separator };
            createMenu.items.Add(item);

            item = new()
            {
                contextItemType = ContextMenuContent.ContextItemType.Button,
                itemIcon = closeGraphIcon,
                itemText = Global.Lang.closeGraph,
                onClick = new()
            };
            item.onClick.AddListener(() => onQuitGraph.Invoke());
            createMenu.items.Add(item);
        }

        void Start()
        {
            Close();
        }

        public void Open(string guid)
        {
            if (guidStack.Count > 0)
                TrySave();

            guidStack.Push(guid);
            Load();
        }

        void Load()
        {
            graphData = Global.Storage.GetGraphData(guidStack.Peek());

            if (openGraphDialog)
                Destroy(openGraphDialog);

            Refresh();

            if (viewport.TryGetComponent(out Viewport v))
            {
                v.enabled = true;
                v.FrameAll();
            }
        }

        public void Close()
        {
            if (guidStack.Count > 0)
            {
                TrySave();
                guidStack.Pop();
            }

            if (guidStack.Count > 0)
            {
                Load();
                return;
            }

            if (viewport.TryGetComponent(out Viewport v))
                v.enabled = false;

            openGraphDialog = Instantiate(openGraphDialogPrefab, canvas.transform);
            if (openGraphDialog.TryGetComponent(out OpenGraphDialog _openGraphDialog))
            {
                _openGraphDialog.graphList.onOpen.AddListener(guid =>
                {
                    _openGraphDialog.onDestroy += () => Open(guid);
                    _openGraphDialog.Stop();
                });
                
                _openGraphDialog.cancelButton.onClick.AddListener(() =>
                {
                    _openGraphDialog.onDestroy += () => Destroy(gameObject);
                    _openGraphDialog.Stop();
                });
            }

            graphData = null;

            foreach (Transform child in nodeParent)
                Destroy(child.gameObject);

            foreach (Transform child in connectionParent)
                Destroy(child.gameObject);
        }

        void Refresh()
        {
            if (graphData.nodeDataList.Count == 0)
            {
                graphData.connectionDataList.Clear();
                graphData.lastNodeId = 0;
                goto GUI;
            }

            graphData.lastNodeId = graphData.nodeDataList.Max(x => x.id);

            var nodeDataIds = new HashSet<long>();
            foreach (var nodeData in graphData.nodeDataList)
            {
                if (nodeData.id == 0 || nodeDataIds.Contains(nodeData.id))
                    nodeData.id = ++graphData.lastNodeId;
                nodeDataIds.Add(nodeData.id);
            }

            graphData.connectionDataList.RemoveAll(x =>
                x.sourceNodeDataId == 0 ||
                !nodeDataIds.Contains(x.sourceNodeDataId) ||
                !nodeDataIds.Contains(x.targetNodeDataId)
            );

        GUI:
            foreach (Transform child in nodeParent)
                Destroy(child.gameObject);

            foreach (Transform child in connectionParent)
                Destroy(child.gameObject);

            foreach (var nodeData in graphData.nodeDataList)
                InstantiateNode(nodeData);

            foreach (var connectionData in graphData.connectionDataList)
                InstantiateConnection(connectionData);
        }

        void InstantiateNode(Node.Data nodeData)
        {
            var node = nodes.FirstOrDefault(x => x && x.type == nodeData.type);
            var gameObject = Instantiate(node.gameObject, nodeParent);
            nodeData.nodeEditor = this;
            nodeData.node = gameObject.GetComponent<Node>();
            nodeData.node.Read(nodeData);
        }

        void InstantiateConnection(Connection.Data connectionData)
        {
            var gameObject = Instantiate(connection.gameObject, connectionParent);

            connectionData.nodeEditor = this;
            connectionData.connection = gameObject.GetComponent<Connection>();
            connectionData.connection.Read(connectionData);
        }

        void Update()
        {
            UpdateSelection();

            if (updateTimer > 0)
            {
                updateTimer -= Time.deltaTime;
                return;
            }

            updateTimer++;

            TrySave();
        }

        void TrySave()
        {
            if (graphData is null)
                return;

            for (int i = 0; i < nodeParent.childCount; i++)
            {
                var t = nodeParent.GetChild(i);
                if (t.TryGetComponent<Node>(out var node))
                    node.Write();
            }

            Global.Storage.SetGraphData(guidStack.Peek(), graphData);
        }

        public void AddNode(string type)
        {
            var position = viewport.transform.InverseTransformPoint(Input.mousePosition);
            graphData.nodeDataList.Add(new() { type = type, position = position });
            Refresh();
        }

        public void ConnectNode(long sourceNodeDataId)
        {
            if (pendingConnectionData is not null)
            {
                if (pendingConnectionData.connection)
                    Destroy(pendingConnectionData.connection.gameObject);
            }

            pendingConnectionData = new() { sourceNodeDataId = sourceNodeDataId };
            InstantiateConnection(pendingConnectionData);
            cancelArea.SetActive(true);
        }

        public void CompleteConnection(long targetNodeDataId)
        {
            if (pendingConnectionData is null)
                return;

            if (!pendingConnectionData.connection)
                return;

            if (pendingConnectionData.sourceNodeDataId == targetNodeDataId)
                return;

            foreach (var connectionData in graphData.connectionDataList)
                if (connectionData.sourceNodeDataId == pendingConnectionData.sourceNodeDataId)
                    if (connectionData.targetNodeDataId == targetNodeDataId)
                        return;

            pendingConnectionData.targetNodeDataId = targetNodeDataId;
            graphData.connectionDataList.Add(pendingConnectionData);
            Refresh();

            cancelArea.SetActive(false);
            pendingConnectionData = null;
        }

        public void CancelConnection()
        {
            if (pendingConnectionData is null)
                return;

            if (!pendingConnectionData.connection)
                return;

            Destroy(pendingConnectionData.connection.gameObject);

            cancelArea.SetActive(false);
            pendingConnectionData = null;
        }

        public void RemoveNode(long nodeDataId)
        {
            graphData.nodeDataList.RemoveAll(x => x.id == nodeDataId);
            Refresh();
        }

        public void RemoveConnection(Connection.Data connectionData)
        {
            connectionData.sourceNodeDataId = 0;
            Refresh();
        }

        public void StartSelection(UnityEngine.EventSystems.BaseEventData baseEventData)
        {
            var e = baseEventData as UnityEngine.EventSystems.PointerEventData;
            if (e.button != 0)
                return;

            selection.gameObject.SetActive(true);
            selectionStart = e.position;
        }

        public void StopSelection(UnityEngine.EventSystems.BaseEventData baseEventData)
        {

            var e = baseEventData as UnityEngine.EventSystems.PointerEventData;
            if (e.button != 0)
                return;

            if (graphData is not null)
                foreach (var nodeData in graphData.nodeDataList)
                    nodeData.node.Deselect();

            if (!selection.gameObject.activeSelf)
                return;

            if (graphData is not null)
            {
                var rect = new Rect(selectionStart, Input.mousePosition - selectionStart);
                foreach (var nodeData in graphData.nodeDataList)
                {
                    if (rect.Contains(nodeData.node.transform.position, true))
                        nodeData.node.Select();
                }
            }

            selection.gameObject.SetActive(false);
        }

        void UpdateSelection()
        {
            if (!selection.gameObject.activeSelf)
                return;

            var start = selectionStart;
            var end = Input.mousePosition;
            Utils.SetRect(selection, start, end);
        }

        public List<Node> GetSelectedNodes()
        {
            return graphData.nodeDataList.Where(x => x.node.selected).Select(x => x.node).ToList();
        }
    }
}

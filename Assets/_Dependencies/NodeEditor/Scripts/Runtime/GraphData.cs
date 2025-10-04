using System;
using System.Collections.Generic;
using System.Linq;

namespace NodeEditor
{
    [Serializable]
    public class GraphData
    {
        public long lastNodeId;

        public List<Node.Data> nodeDataList = new();
        public List<Connection.Data> connectionDataList = new();

        public Node.Data GetNode(string type)
        {
            return nodeDataList.FirstOrDefault(x => x.type == type);
        }

        public List<Node.Data> GetNodes(string type)
        {
            return nodeDataList.Where(x => x.type == type).ToList();
        }
    }
}
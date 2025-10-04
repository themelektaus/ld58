using UnityEngine;

namespace NodeEditor
{
    public static class Global
    {
        public static class Lang
        {
            public const string closeGraph = "Close Graph";
        }

        public static class RegistryKey
        {
            public static string ActiveGraphGUID()
            {
                return $"ActiveGraphGUID";
            }

            public static string GraphGUIDs()
            {
                return $"GraphGUIDs";
            }

            public static string Graph(string guid)
            {
                return $"Graph {guid}";
            }

            public static string GraphTitle(string guid)
            {
                return $"Graph {guid} (Title)";
            }
        }

        public static class Storage
        {
            public static GraphData GetGraphData(string guid)
            {
                var json = PlayerPrefs.GetString(RegistryKey.Graph(guid));                

                if (string.IsNullOrEmpty(json))
                    return null;

                return JsonUtility.FromJson<GraphData>(json);
            }

            public static void SetGraphData(string guid, GraphData graphData)
            {
                SetGraphJson(guid, JsonUtility.ToJson(graphData));
            }

            static void SetGraphJson(string guid, string graphJson)
            {
                PlayerPrefs.SetString(RegistryKey.Graph(guid), graphJson);
            }

            public static StringList GetGraphGUIDs()
            {
                return JsonUtility.FromJson<StringList>(PlayerPrefs.GetString(RegistryKey.GraphGUIDs())) ?? new();
            }

            public static void SetGraphGUIDs(StringList stringList)
            {
                PlayerPrefs.SetString(RegistryKey.GraphGUIDs(), JsonUtility.ToJson(stringList));
            }

            public static string GetActiveGraphGUID()
            {
                return PlayerPrefs.GetString(RegistryKey.ActiveGraphGUID());
            }

            public static void SetActiveGraphGUID(string guid)
            {
                PlayerPrefs.SetString(RegistryKey.ActiveGraphGUID(), guid);
            }

            public static string GetGraphTitle(string guid)
            {
                return PlayerPrefs.GetString(RegistryKey.GraphTitle(guid));
            }

            public static void SetGraphTitle(string guid, string title)
            {
                PlayerPrefs.SetString(RegistryKey.GraphTitle(guid), title);
            }

            public static string CreateGraph(string title)
            {
                return CreateGraph(title, JsonUtility.ToJson(new GraphData()));
            }

            public static string CreateGraph(string title, string graphJson)
            {
                var graphGUIDs = GetGraphGUIDs();
                var guid = graphGUIDs.AddGuid();
                SetGraphJson(guid, graphJson);
                SetGraphTitle(guid, title);
                SetGraphGUIDs(graphGUIDs);
                return guid;
            }

            public static void DeleteGraph(string guid)
            {
                var guids = GetGraphGUIDs();
                guids.items.RemoveAll(x => x == guid);
                PlayerPrefs.DeleteKey(RegistryKey.Graph(guid));
                PlayerPrefs.DeleteKey(RegistryKey.GraphTitle(guid));
                SetGraphGUIDs(guids);
            }
        }
    }
}
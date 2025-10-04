using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.Editor
{
    public class MonitorWindow : EditorWindow
    {
        [MenuItem(Const.MENU_ASSETS + "/Monitor")]
        public static void Open()
        {
            _ = GetWindow<MonitorWindow>("Monitor");
        }

        [SerializeField] VisualTreeAsset visualTreeAsset;

        VisualElement root;

        ListView conditionsListView;
        ListView queriesListView;

        readonly List<Condition> conditions = new();
        readonly List<(ObjectQuery, List<Object>)> queries = new();

        public void CreateGUI()
        {
            if (!visualTreeAsset)
                return;

            root = rootVisualElement;
            root.AddVisualTreeAsset(visualTreeAsset);

            static Label MakeItem() => new()
            {
                style = {
                    paddingLeft = 5,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            conditionsListView = root.Q<ListView>("Conditions");
            conditionsListView.itemsSource = conditions;
            conditionsListView.makeItem = MakeItem;
            conditionsListView.bindItem = (item, index) =>
            {
                var label = item as Label;
                var condition = conditions[index];
                if (condition)
                {
                    label.SetEnabled(true);
                    label.text = condition.transform.GetPath();

                    // MyTODO: Extract TD_Interactable
                    if (condition.GetType().Name == "TD_Interactable")
                        label.text = $"<color=yellow>{label.text}</color>";

                    return;
                }
                label.SetEnabled(false);
            };
            conditionsListView.selectionChanged += x =>
            {
                var condition = x.FirstOrDefault() as Condition;
                if (condition)
                {
                    Selection.activeObject = condition;
                    EditorGUIUtility.PingObject(condition);
                    return;
                }
                conditionsListView.selectedIndex = -1;
            };

            queriesListView = root.Q<ListView>("Queries");
            queriesListView.itemsSource = queries;
            queriesListView.makeItem = MakeItem;
            queriesListView.bindItem = (item, index) =>
            {
                var label = item as Label;
                var (query, objects) = queries[index];
                if (query)
                {
                    label.SetEnabled(true);
                    label.text = $"{query.name} (Found {objects.Count} Objects)";
                    return;
                }
                label.SetEnabled(false);
            };
            queriesListView.selectionChanged += x =>
            {
                var (query, objects) = ((ObjectQuery, List<Object>)) x.FirstOrDefault();
                if (query)
                {
                    Selection.objects = objects.Select(x => x.GetGameObject()).ToArray();
                    if (objects.Count == 1)
                        EditorGUIUtility.PingObject(objects.First());
                    return;
                }
                queriesListView.selectedIndex = -1;
            };
        }

        readonly Timer timer = new();

        void Update()
        {
            if (root is null)
                return;

            if (!timer.Update(.25f))
                return;

            var conditionList = Monitor.conditions.Where(x => x).ToList();
            if (conditionList.Count == 0)
                conditionList.Add(null);
            else
                conditionList.Sort((a, b) => Utils.CompareHierarchy(a.transform, b.transform));

            if (!conditions.SequenceEqual(conditionList))
            {
                conditions.Clear();
                conditions.AddRange(conditionList);
                conditionsListView.Rebuild();
            }

            var queryList = Monitor.queries
                .Where(x => x.Value.All(x => x))
                .Select(x => (x.Key, x.Value))
                .ToList();

            if (queryList.Count == 0)
                queryList.Add((null, new()));

            if (!queries.SequenceEqual(queryList))
            {
                queries.Clear();
                queries.AddRange(queryList);
                queriesListView.Rebuild();
            }
        }
    }
}

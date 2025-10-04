using UnityEngine;

namespace NodeEditor
{
    [RequireComponent(typeof(Node))]
    public class Node_Subgraph : MonoBehaviour
    {
        Node node;

        [SerializeField] CustomDropdown dropdown;
        [SerializeField] UnityEngine.UI.Button editButton;

        void Awake()
        {
            node = GetComponent<Node>();

            var guidList = Global.Storage.GetGraphGUIDs();

            dropdown.dropdownItems.Clear();

            foreach (var guid in guidList.items)
            {
                var _guid = guid;
                dropdown.dropdownItems.Add(new()
                {
                    itemName = Global.Storage.GetGraphTitle(_guid)
                });
            }

            node.onRead += () =>
            {
                var guid = node.data.GetString("guid");
                dropdown.SetIndex(Mathf.Max(0, guidList.items.IndexOf(guid)));
            };

            node.onWrite += () =>
            {
                node.data.SetString("guid", guidList.items[dropdown.GetIndex()]);
            };

            editButton.onClick.AddListener(() =>
            {
                node.data.nodeEditor.Open(guidList.items[dropdown.GetIndex()]);
            });
        }
    }
}
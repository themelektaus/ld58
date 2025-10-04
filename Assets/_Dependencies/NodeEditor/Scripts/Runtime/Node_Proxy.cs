using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    [RequireComponent(typeof(Node))]
    public class Node_Proxy : MonoBehaviour
    {
        Node node;

        [SerializeField] CustomDropdown dropdown;
        
        void Awake()
        {
            node = GetComponent<Node>();
            
            node.onRead += () =>
            {
                dropdown.SetText(node.data.GetString("name"), () =>
                    FindObjectsByType<Node_Proxy>(FindObjectsSortMode.None)
                        .Select(x => x.dropdown.GetText())
                        .ToList()
                );
            };
            
            node.onWrite += () =>
            {
                node.data.SetString("name", dropdown.GetText());
            };
        }
    }
}

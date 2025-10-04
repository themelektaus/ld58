using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NodeEditor
{
    public class GraphList : MonoBehaviour
    {
        [SerializeField] GraphListItem itemPrefab;
        [SerializeField] RectTransform listTransform;

        [SerializeField] UnityEngine.UI.Button createButton;

        public UnityEvent<string> onOpen;

        readonly List<GraphListItem> items = new();

        void Awake()
        {
            createButton.onClick.AddListener(() =>
            {
                Global.Storage.CreateGraph("New Graph");
                Refresh();
            });
        }

        void OnEnable()
        {
            Refresh();
        }

        void OnDisable()
        {
            Clear();
        }

        void Clear()
        {
            items.Clear();
            foreach (Transform child in listTransform)
                Destroy(child.gameObject);
        }

        void Refresh()
        {
            Clear();

            var guids = LoadGraphGUIDs();

            var activeGuid = Global.Storage.GetActiveGraphGUID();

            foreach (var guid in guids.items)
            {
                var _guid = guid;
                
                var itemGameObject = Instantiate(itemPrefab.gameObject, listTransform);
                
                var newItem = itemGameObject.GetComponent<GraphListItem>();
                newItem.inputField.inputText.text = Global.Storage.GetGraphTitle(guid);
                newItem.inputField.inputText.onValueChanged.AddListener((UnityAction<string>) (title =>
                {
                    Global.Storage.SetGraphTitle(guid, title);
                }));
                newItem.inputField.UpdateStateInstant();

                if (guid == activeGuid)
                {
                    newItem.activateButton.gameObject.SetActive(false);
                }
                else
                {
                    newItem.activateButton.events.click.AddListener(() =>
                    {
                        Global.Storage.SetActiveGraphGUID(guid);
                        Refresh();
                    });
                }

                newItem.openButton.events.click.AddListener(() =>
                {
                    onOpen.Invoke(_guid);
                });

                newItem.deleteButton.events.click.AddListener(() =>
                {
                    Global.Storage.DeleteGraph(guid);
                    guids = LoadGraphGUIDs();
                    Refresh();
                });

                items.Add(newItem);
            }
        }

        StringList LoadGraphGUIDs()
        {
            return Global.Storage.GetGraphGUIDs();
        }
    }
}

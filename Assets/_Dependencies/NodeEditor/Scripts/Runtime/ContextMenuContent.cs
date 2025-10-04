using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace NodeEditor
{
    public class ContextMenuContent : MonoBehaviour, IPointerDownHandler
    {
        [FormerlySerializedAs("contexItems")]
        public List<ContextItem> items = new();

        ContextMenuManager manager;
        
        [System.Serializable]
        public class ContextItem
        {
            [Header("Information")]
            public string itemText = "Item Text";
            public Sprite itemIcon;
            public ContextItemType contextItemType;

            [Header("Events")]
            public UnityEvent onClick;
        }

        public enum ContextItemType { Button, Separator }

        void Awake()
        {
            manager = FindFirstObjectByType<ContextMenuManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            manager.Close();
            if (eventData.button == PointerEventData.InputButton.Right)
                manager.Open(items);
        }

        public void CloseOnClick()
        {
            manager.isOn = false;

            if (!manager.animator.isActiveAndEnabled)
                return;

            manager.animator.Play("Menu Out");
        }
    }
}

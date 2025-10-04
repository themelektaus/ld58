using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using ContextItem = NodeEditor.ContextMenuContent.ContextItem;
using ContextItemType = NodeEditor.ContextMenuContent.ContextItemType;

namespace NodeEditor
{
    [RequireComponent(typeof(Animator))]
    public class ContextMenuManager : MonoBehaviour
    {
        static Camera mainCamera => Camera.main;

        [FormerlySerializedAs("contextContent")]
        public GameObject content;

        [FormerlySerializedAs("contextButton")]
        public GameObject button;

        [FormerlySerializedAs("contextSeparator")]
        public GameObject separator;

        [FormerlySerializedAs("vBorderTop")]
        [Range(-50, 50)] public int top = -10;

        [FormerlySerializedAs("vBorderBottom")]
        [Range(-50, 50)] public int bottom = 10;

        [FormerlySerializedAs("hBorderLeft")]
        [Range(-50, 50)] public int left = 15;

        [FormerlySerializedAs("hBorderRight")]
        [Range(-50, 50)] public int right = -15;

        Canvas mainCanvas;
        [System.NonSerialized] public Animator animator;

        Vector3 cursorPosition;
        Vector3 contentPosition;
        
        Transform itemParent;

        [HideInInspector] public bool isOn;

        public enum CameraSource { Main, Custom }

        public enum SubMenuBehaviour { Hover, Click }

        void Awake()
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
            animator = gameObject.GetComponent<Animator>();

            contentPosition = new(top, left, 0);
            gameObject.transform.SetAsLastSibling();

            itemParent = transform.Find("Content/Item List").transform;
        }

        void OnEnable()
        {
            animator.Play("Start");
            Init();
        }

        void Init()
        {
            var scale = content.transform.localScale;
            scale.y = 0;
            content.transform.localScale = scale;
        }

        public void SetContextMenuPosition(Vector2 offset)
        {
            cursorPosition = Input.mousePosition;

            float x = offset.x * Screen.width / 1920;
            float y = offset.y * Screen.height / 1080;

            if (mainCanvas.renderMode == RenderMode.ScreenSpaceCamera || mainCanvas.renderMode == RenderMode.WorldSpace)
                content.transform.position = mainCamera.ScreenToWorldPoint(cursorPosition);

            else if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                content.transform.position = new(cursorPosition.x + x, cursorPosition.y + y, 0);

            Init();
        }

        public void OpenContextMenu() => Open();

        public void Open()
        {
            Init();
            animator.Play("Menu In");
            isOn = true;
        }

        public void Open(List<ContextItem> contextItems, Vector2 offset = new())
        {
            Close();

            foreach (Transform child in itemParent)
                Destroy(child.gameObject);

            foreach (var contextItem in contextItems)
            {
                switch (contextItem.contextItemType)
                {
                    case ContextItemType.Button:
                        var buttonInstance = Instantiate(button);
                        buttonInstance.transform.SetParent(itemParent, false);

                        if (buttonInstance.TryGetComponent(out CustomButton customButton))
                        {
                            customButton.appearance.icon = contextItem.itemIcon;
                            customButton.appearance.text = contextItem.itemText;
                            customButton.UpdateAppearance();

                            customButton.events.click.AddListener(contextItem.onClick.Invoke);
                            customButton.events.click.AddListener(Close);
                        }
                        break;

                    case ContextItemType.Separator:
                        var separatorInstance = Instantiate(separator);
                        separatorInstance.transform.SetParent(itemParent, false);
                        break;
                }

                StopCoroutine(nameof(ExecuteAfterTime));
                StartCoroutine(nameof(ExecuteAfterTime), .01f);
            }

            SetContextMenuPosition(offset);
            Open();
            SetContextMenuPosition(offset);
        }

        public void CloseOnClick(UnityEngine.EventSystems.BaseEventData _) => Close();

        public void Close()
        {
            if (!isOn)
                return;

            animator.Play("Menu Out");
            isOn = false;
        }

        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            itemParent.gameObject.SetActive(false);
            itemParent.gameObject.SetActive(true);
        }
    }
}

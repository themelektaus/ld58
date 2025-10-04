using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

using System;
using System.Collections;

namespace NodeEditor
{
    [RequireComponent(typeof(Button))]
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        static float lastClick;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitializeOnLoad() => lastClick = 0;

        [SerializeField] Theme theme;

        [FormerlySerializedAs("normalImage")]
        public Image iconImage;

        [FormerlySerializedAs("normalText")]
        public TextMeshProUGUI uiText;

        public GameObject rippleParent;

        [Serializable]
        public class Appearance
        {
            public int colorIndex;
            public Sprite icon;
            public string text;
        }
        public Appearance appearance = new();

        [Serializable]
        public class Events
        {
            public UnityEvent click;
            public UnityEvent hover;
        }
        public Events events = new();

        public CanvasGroup canvasGroup { get; private set; }
        public Button button { get; private set; }
        public Image image { get; private set; }

        public bool isHovering { get; private set; }

        public enum RippleUpdateMode { Normal, UnscaledTime }

        void OnValidate()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            button = GetComponent<Button>();
            image = GetComponent<Image>();

            UpdateAppearance();
        }

        public void UpdateAppearance()
        {
            if (iconImage)
            {
                if (appearance.icon)
                {
                    iconImage.enabled = true;
                    iconImage.sprite = appearance.icon;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (uiText)
            {
                var m = uiText.margin;
                m.x = appearance.icon ? 35 : 0;
                uiText.margin = m;
                uiText.text = appearance.text;
            }

            if (!isActiveAndEnabled)
                return;

            IEnumerator _()
            {
                yield return null;
                UpdateTheme();
            }
            StartCoroutine(_());
        }

        void Awake()
        {
            OnValidate();

            if (!theme)
            {
                Debug.LogWarning("Theme is null", this);
            }

            if (rippleParent)
            {
                if (theme && theme.button.rippleShape)
                {
                    rippleParent.SetActive(false);
                    return;
                }

                Destroy(rippleParent);
                rippleParent = null;
            }
        }

        bool IsInteractable()
            => button.interactable && Utils.IsInteractable(transform.parent);

        void Update()
        {
            UpdateTheme();

            if (!canvasGroup || !button)
                return;

            if (IsInteractable())
            {
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1;
                return;
            }

            canvasGroup.interactable = false;
            canvasGroup.alpha = .3f;
        }

        void UpdateTheme()
        {
            if (theme)
                theme.button.ApplyTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable || !IsInteractable() || !isHovering)
                return;

            if (!theme || !theme.button.rippleShape || !rippleParent)
            {
                events.click.Invoke();
                return;
            }

            theme.button.CreateRipple(rippleParent, Input.mousePosition);

            if (lastClick + .2f <= Time.unscaledTime)
            {
                lastClick = Time.unscaledTime;
                IEnumerator _()
                {
                    yield return new WaitForSecondsRealtime(.2f);
                    if (IsInteractable())
                        events.click.Invoke();
                }
                StartCoroutine(_());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            events.hover.Invoke();
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
        }
    }
}

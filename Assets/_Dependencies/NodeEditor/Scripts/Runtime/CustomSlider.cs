using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using TMPro;

namespace NodeEditor
{
    [RequireComponent(typeof(Slider))]
    public class CustomSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        // Resources
        public Slider mainSlider;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI popupValueText;

        // Settings
        public string suffix = "";
        public bool showValue = true;
        public bool showPopupValue = false;
        public bool useRoundValue = false;
        public bool usePercentage = false;

        // Events
        [System.Serializable] public class SliderEvent : UnityEvent<float> { }
        [Space(8)] public SliderEvent sliderEvent;
        public UnityEvent rightClickEvent;
        public UnityEvent doubleClickEvent;

        // Other Variables
        [HideInInspector] public Animator sliderAnimator;

        float lastUpTime = -1;

        void Awake()
        {
            mainSlider.onValueChanged.AddListener(delegate
            {
                sliderEvent.Invoke(mainSlider.value);
                UpdateUI();
            });

            sliderAnimator = gameObject.GetComponent<Animator>();

            sliderEvent.Invoke(mainSlider.value);

            UpdateUI();
        }

        public void UpdateUI()
        {
            float v = mainSlider.value;

            string text;

            if (useRoundValue)
                text = $"{Mathf.Round(v)}{suffix}";
            else if (usePercentage)
                text = $"{Mathf.Round(v * 100)}{suffix}";
            else
                text = $"{v:F1}{suffix}";

            if (valueText)
                valueText.text = text;

            if (popupValueText)
                popupValueText.text = text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!showPopupValue)
                return;

            if (sliderAnimator)
                sliderAnimator.Play("Value In");
            else
                popupValueText.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!showPopupValue)
                return;

            if (sliderAnimator)
                sliderAnimator.Play("Value Out");
            else
                popupValueText.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var time = Time.unscaledTime;
                if (time - lastUpTime > .2f)
                {
                    lastUpTime = time;
                    return;
                }

                lastUpTime = -1;
                doubleClickEvent.Invoke();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                rightClickEvent.Invoke();
                return;
            }
        }
    }
}

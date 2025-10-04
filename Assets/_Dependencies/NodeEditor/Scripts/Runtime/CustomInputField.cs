using TMPro;

using UnityEngine;
using UnityEngine.Events;

namespace NodeEditor
{
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(Animator))]
    public class CustomInputField : MonoBehaviour
    {
        [Header("Resources")]
        public TMP_InputField inputText;
        public Animator inputFieldAnimator;

        [Header("Events")]
        public UnityEvent<string> onInput;

        void Awake()
        {
            if (!inputText)
                inputText = gameObject.GetComponent<TMP_InputField>();

            if (!inputFieldAnimator)
                inputFieldAnimator = gameObject.GetComponent<Animator>();

            inputText.onSelect.AddListener(_ => AnimateIn());
            inputText.onEndEdit.AddListener(_ => AnimateOut());
            inputText.onValueChanged.AddListener(x => onInput.Invoke(x));

            UpdateStateInstant();
        }

        void OnEnable()
        {
            if (!inputText)
                return;

            inputText.ForceLabelUpdate();
            UpdateStateInstant();
        }

        public void AnimateIn()
        {
            if (!inputFieldAnimator.gameObject.activeInHierarchy)
                return;

            inputFieldAnimator.enabled = true;
            inputFieldAnimator.Play("In");
        }

        public void AnimateOut()
        {
            if (!inputFieldAnimator.gameObject.activeInHierarchy)
                return;

            inputFieldAnimator.enabled = true;

            if (inputText.text.Length == 0)
                inputFieldAnimator.Play("Out");
        }

        public void UpdateState()
        {
            if (inputText.text.Length == 0)
            {
                AnimateOut();
                return;
            }

            AnimateIn();
        }

        public void UpdateStateInstant()
        {
            if (inputText.text.Length == 0)
            {
                inputFieldAnimator.Play("Out", 0, 1);
                return;
            }

            inputFieldAnimator.Play("In", 0, 1);
        }

        public string GetText()
        {
            return inputText.text;
        }

        public void SetText(string text)
        {
            inputText.text = text;
            UpdateStateInstant();
        }
    }
}
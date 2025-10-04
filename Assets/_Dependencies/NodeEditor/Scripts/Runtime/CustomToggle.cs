using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Animator))]
    public class CustomToggle : MonoBehaviour
    {
        [HideInInspector] public Toggle toggleObject;
        [HideInInspector] public Animator toggleAnimator;

        void Awake()
        {
            Init();
            UpdateState();
        }

        void Init()
        {
            if (!toggleObject)
            {
                toggleObject = gameObject.GetComponent<Toggle>();
                toggleObject.onValueChanged.AddListener(UpdateStateDynamic);
            }

            if (!toggleAnimator)
                toggleAnimator = toggleObject.GetComponent<Animator>();
        }

        public void UpdateState()
        {
            StartCoroutine(DisableAnimator());
            toggleAnimator.enabled = true;
            toggleAnimator.Play($"{(toggleObject.isOn ? "On" : "Off")} Instant");
            StartCoroutine(DisableAnimator());
        }

        public void UpdateStateDynamic(bool value)
        {
            StopCoroutine(DisableAnimator());
            toggleAnimator.enabled = true;
            toggleAnimator.Play($"Toggle {(toggleObject.isOn ? "On" : "Off")}");
            StopCoroutine(DisableAnimator());
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(.5f);
            toggleAnimator.enabled = false;
        }

        public bool Get()
        {
            Init();
            return toggleObject.isOn;
        }

        public void Set(bool value)
        {
            Init();
            toggleObject.isOn = value;
            UpdateState();
        }
    }
}
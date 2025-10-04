using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Prototype
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Button (UI)")]
    public class ButtonUI : MouseBehaviourUI, IDestroyable
    {
        public bool fakeSelected;
        public bool disabled;

        [SerializeField] float destroyDelay = 1;

        [SerializeField] SoundEffect hoverSoundEffect;
        [SerializeField] SoundEffect clickSoundEffect;

        [SerializeField] string visibleBool = "Visible";
        [SerializeField] string hoverBool = "Hover";
        [SerializeField] string clickTrigger = "Click";
        [SerializeField] string clickLayer = "Click Layer";

        public UnityEvent onLeftClick;
        public UnityEvent onRightClick;

        [NonSerialized] public Animator animator;

        bool firstUpdate;
        bool hoverBoolValue;

        bool mouseMode;

        int clickLayerIndex = -1;
        float clickLayerWeight;

        void Awake()
        {
            animator = GetComponent<Animator>();
            animator.keepAnimatorStateOnDisable = true;

            if (animator.GetParameterNames().Contains(visibleBool))
                animator.SetBool(visibleBool, true);

            if (TryGetComponent(out Selectable selectable))
                if (selectable.navigation.mode == Navigation.Mode.None)
                    mouseMode = true;
        }

        void OnEnable()
        {
            clickLayerIndex = animator.GetLayerIndex(clickLayer);
            firstUpdate = true;
        }

        protected override void OnDisable()
        {
            animator.SetBool(hoverBool, false);

            base.OnDisable();
        }

        protected override void Update()
        {
            UpdateLayers();

            if (mouseMode)
            {
                var e = EventSystem.current;

                if (!e.currentSelectedGameObject)
                {
                    var results = new List<RaycastResult>();
                    e.RaycastAll(new(e) { position = Input.mousePosition }, results);

                    foreach (var result in results)
                    {
                        var transform = result.gameObject.transform;
                        while (transform)
                        {
                            if (transform.TryGetComponent(out ButtonUI button))
                            {
                                if (button == this)
                                {
                                    OnPointerEnter();
                                    return;
                                }
                            }
                            transform = transform.parent;
                        }
                    }
                }
            }

            base.Update();

            var hoverBoolValue = fakeSelected || isSelected || clickLayerWeight > 0;
            if (this.hoverBoolValue != hoverBoolValue)
            {
                this.hoverBoolValue = hoverBoolValue;
                if (hoverBoolValue && hoverSoundEffect && !firstUpdate)
                    hoverSoundEffect.Play();
            }
            animator.SetBool(hoverBool, hoverBoolValue);

            firstUpdate = false;
        }

        void UpdateLayers()
        {
            if (clickLayerIndex != -1)
            {
                animator.SetLayerWeight(clickLayerIndex, clickLayerWeight);
                clickLayerWeight = Mathf.Max(0, clickLayerWeight - Time.deltaTime * 6);
            }
        }

        protected override void OnDown(int button)
        {
            if (!enabled)
                return;

            if (disabled)
                return;

            if (!isInteractable)
                return;

            switch (button)
            {
                case 0:
                    onLeftClick.Invoke();

                    if (clickTrigger != string.Empty)
                        animator.SetTrigger(clickTrigger);

                    if (clickLayerIndex != -1)
                        clickLayerWeight = 1;

                    if (clickSoundEffect)
                        clickSoundEffect.Play();
                    break;

                case 1:
                    onRightClick.Invoke();
                    break;
            }
        }

        protected override void OnLeave()
        {
            if (mouseMode)
            {
                if (isSelected)
                    EventSystem.current.SetSelectedGameObject(null);
            }
        }

        void LateUpdate()
        {
            if (clickTrigger != string.Empty)
                animator.ResetTrigger(clickTrigger);
        }

        public void PerformLeftClick() => OnDown(0);
        public void PerformRightClick() => OnDown(1);

        public void Destroy()
        {
            if (destroyDelay == 0)
            {
                gameObject.Kill();
                return;
            }

            this.CreateSequence(() =>
                {
                    if (animator.GetParameterNames().Contains(visibleBool))
                        animator.SetBool(visibleBool, false);
                })
                .Wait(destroyDelay)
                .Kill(gameObject)
                .Start();
        }
    }
}

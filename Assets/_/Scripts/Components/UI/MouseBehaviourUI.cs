using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Prototype
{
    public abstract class MouseBehaviourUI : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        protected HashSet<int> buttons { get; private set; } = new();

        public bool isHovered { get; private set; }

        public bool isSelected => this.IsSelected();

        protected bool isInteractable => gameObject.IsInteractableRecursive();

        protected virtual void OnDisable()
        {
            if (isSelected)
            {
                foreach (var button in buttons)
                    OnUp(button);

                Leave();
            }

            buttons.Clear();
        }

        protected virtual void Update()
        {
            if (isSelected && !isInteractable)
            {
                EventSystem.current.SetSelectedGameObject(null);
                OnDisable();
            }
        }

        public void OnPointerEnter()
        {
            OnPointerEnter(default);
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (!isInteractable)
                return;

            this.Select();

            Enter();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData _)
        {
            Leave();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable)
                return;

            this.Select();

            var button = (int) eventData.button;
            buttons.Add(button);
            OnDown(button);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            var button = (int) eventData.button;
            buttons.Remove(button);
            OnUp(button);
        }

        protected virtual void OnEnter() { }
        protected virtual void OnLeave() { }
        protected virtual void OnDown(int button) { }
        protected virtual void OnUp(int button) { }

        void Enter()
        {
            isHovered = true;
            OnEnter();
        }

        void Leave()
        {
            isHovered = false;
            OnLeave();
        }
    }
}

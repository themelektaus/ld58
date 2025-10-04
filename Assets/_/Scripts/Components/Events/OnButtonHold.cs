using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Button Hold")]
    public class OnButtonHold : On<float>
    {
        [SerializeField, HideInInspector] string button;
        [SerializeField, HideInInspector] string alternativeButton = "";

        [SerializeField] string[] buttons = new[] { "Fire1" };

        [SerializeField] bool needsRelease;

        bool released;
        float? downTime;

        void Awake()
        {
            if (button != string.Empty)
                this.LogWarning($"button \"{button}\" is deprecated");

            if (alternativeButton != string.Empty)
                this.LogWarning($"alternative button \"{alternativeButton}\" is deprecated");
        }

        void OnEnable()
        {
            released = false;
        }

        void OnDisable()
        {
            downTime = null;
        }

        void Update()
        {
            var anyButtonDown = buttons.Where(x => !x.IsNullOrEmpty()).Any(Input.GetButton);

            if (!released && needsRelease)
            {
                if (anyButtonDown)
                    return;

                released = true;
            }

            if (anyButtonDown)
            {
                Invoke(Time.time - (downTime ??= Time.time));
                return;
            }

            OnDisable();
        }
    }
}

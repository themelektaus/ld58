using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Button Down")]
    public class OnButtonDown : On
    {
        [SerializeField, HideInInspector] string button;
        [SerializeField, HideInInspector] string alternativeButton;

        [SerializeField] string[] buttons = new[] { "Fire1" };
        [SerializeField] bool ignoreMouse;

        void Awake()
        {
            if (button != string.Empty)
                this.LogWarning($"button \"{button}\" is deprecated");

            if (alternativeButton != string.Empty)
                this.LogWarning($"alternative button \"{alternativeButton}\" is deprecated");
        }

        void Update()
        {
            if (ignoreMouse)
                for (var i = 0; i <= 2; i++)
                    if (Input.GetMouseButtonDown(i))
                        return;

            var buttons = this.buttons
                .Append(button)
                .Append(alternativeButton)
                .Where(x => !x.IsNullOrEmpty());

            if (!buttons.Any(Input.GetButtonDown))
                return;

            Invoke();
        }
    }
}

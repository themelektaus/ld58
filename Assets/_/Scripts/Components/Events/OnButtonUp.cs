using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_EVENTS + "On Button Up")]
    public class OnButtonUp : On
    {
        [SerializeField, HideInInspector] string button;
        [FormerlySerializedAs("alternativeButton")]
        [SerializeField, HideInInspector] string alternativeButton1 = string.Empty;
        [SerializeField, HideInInspector] string alternativeButton2 = string.Empty;
        [SerializeField, HideInInspector] string alternativeButton3 = string.Empty;

        [SerializeField] string[] buttons = new[] { "Fire1" };

        void Awake()
        {
            if (button != string.Empty)
                this.LogWarning($"button \"{button}\" is deprecated");

            if (alternativeButton1 != string.Empty)
                this.LogWarning($"alternative button \"{alternativeButton1}\" is deprecated");

            if (alternativeButton1 != string.Empty)
                this.LogWarning($"alternative button \"{alternativeButton2}\" is deprecated");

            if (alternativeButton1 != string.Empty)
                this.LogWarning($"alternative button \"{alternativeButton3}\" is deprecated");
        }

        void Update()
        {
            var buttons = this.buttons
                .Append(button)
                .Append(alternativeButton1)
                .Append(alternativeButton2)
                .Append(alternativeButton3)
                .Where(x => !x.IsNullOrEmpty());

            if (!buttons.Any(Input.GetButtonUp))
                return;

            Invoke();
        }
    }
}

using NodeEditor;

using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(CustomToggle))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Custom Toggle Binding (UI)")]
    public class CustomToggleBindingUI : MonoBehaviour
    {
        [SerializeField] Reference boolReference;

        CustomToggle toggle;

        void Awake()
        {
            toggle = GetComponent<CustomToggle>();
        }

        void OnEnable()
        {
            toggle.Set(boolReference.Get<bool>());
        }

        void Update()
        {
            boolReference.Set(toggle.Get());
        }
    }
}

using UnityEngine;

namespace Prototype
{
    using SliderUI = UnityEngine.UI.Slider;

    [RequireComponent(typeof(SliderUI))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Slider Binding (UI)")]
    public class SliderBindingUI : MonoBehaviour
    {
        [SerializeField] Reference floatReference;

        SliderUI slider;

        void Awake()
        {
            slider = GetComponent<SliderUI>();
        }

        void OnEnable()
        {
            slider.value = floatReference.Get<float>();
        }

        void Update()
        {
            floatReference.Set(slider.value);
        }
    }
}

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Audio Mixer Slider Binding (UI)")]
    public class AudioMixerSliderBindingUI : MonoBehaviour
    {
        static readonly Dictionary<string, float> defaultValues = new();

        [SerializeField] AudioMixer mixer;
        [SerializeField] string parameter;
        [SerializeField] bool persistent;

        UnityEngine.UI.Slider slider;

        string key => $"{mixer.name}__{parameter}";

        float decibel;

        void Awake()
        {
            if (!defaultValues.ContainsKey(key))
            {
                defaultValues.Add(key, 0);
                if (mixer.GetFloat(parameter, out var defaultValue))
                    defaultValues[key] = defaultValue;
            }

            slider = GetComponent<UnityEngine.UI.Slider>();

            if (!persistent)
                return;

            if (PlayerPrefs.HasKey(key))
            {
                decibel = PlayerPrefs.GetFloat(key);
                mixer.SetFloat(parameter, decibel);
            }
        }

        void OnEnable()
        {
            if (mixer.GetFloat(parameter, out float decibel))
                slider.value = FromDecibel(decibel);
        }

        void Update()
        {
            var decibel = ToDecibel(slider.value);
            mixer.SetFloat(parameter, decibel);

            if (this.decibel == decibel)
                return;

            this.decibel = decibel;

            if (!persistent)
                return;

            PlayerPrefs.SetFloat(key, decibel);
            PlayerPrefs.Save();
        }

        public void ResetToDefaultValue()
        {
            slider.value = FromDecibel(defaultValues[key]);
        }

        static float FromDecibel(float decibel)
        {
            return Mathf.Pow(10, decibel / 80);
        }

        static float ToDecibel(float value)
        {
            return Mathf.Log10(Mathf.Max(value, .0001f)) * 80;
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

namespace Prototype
{
    [CreateAssetMenu(menuName = "Prototype/Sound Effect")]
    public class SoundEffect : UnityEngine.ScriptableObject
    {
        public SoundEffectCollection collection;
        [SerializeField] SoundEffectDistanceRange distanceRange;
        [SerializeField] Vector2 volumeMultiplier = new(1, 1);
        [SerializeField] Vector2 pitch = new(.95f, 1.05f);

        [Header("Additional Settings")]
        public AudioMixerGroup audioMixerGroup;
        public float volumeMultiplierFactor = 1;
        [SerializeField] bool loop;
        [SerializeField] Vector2 fade = new();

        public SoundEffectOptions ToOptions()
        {
            return new()
            {
                audioMixerGroup = audioMixerGroup,
                volumeMultiplier = Utils.RandomRange(volumeMultiplier) * volumeMultiplierFactor,
                pitch = Utils.RandomRange(pitch),
                loop = loop,
                distanceRange = distanceRange ? distanceRange.range : null,
                fade = fade
            };
        }

#if UNITY_EDITOR
        public void ApplyVolumeMultiplierFactor()
        {
            volumeMultiplier *= volumeMultiplierFactor;
            volumeMultiplierFactor = 1;
        }
#endif

        public void Play() => _ = PlayClip();
        public void PlayClipAt(Transform transform) => _ = PlayClipAt(transform.position);
        public void PlayRandom() => _ = PlayRandomClip();
        public void PlayRandomClipAt(Transform transform) => _ = PlayRandomClipAt(transform.position);

        public SoundEffectInstance PlayClip()
        {
            return PlayClip(destroyOnLoad: true);
        }

        public SoundEffectInstance PlayClip(bool destroyOnLoad)
        {
            var options = ToOptions();
            options.destroyOnLoad = destroyOnLoad;
            return collection.PlayClipAt(new(), options);
        }

        public SoundEffectInstance PlayClipAt(Vector3 position)
        {
            return collection.PlayClipAt(position, ToOptions());
        }

        public SoundEffectInstance PlayClipWithDelayAt(Vector3 position, float delay)
        {
            var options = ToOptions();
            options.delay = delay;
            return collection.PlayClipAt(position, options);
        }

        public SoundEffectInstance PlayRandomClip()
        {
            return collection.PlayRandomClip(ToOptions());
        }

        public SoundEffectInstance PlayRandomClipAt(Vector3 position)
        {
            return collection.PlayRandomClipAt(position, ToOptions());
        }

        public SoundEffectInstance PlayRandomClipWithDelayAt(Vector3 position, float delay)
        {
            var options = ToOptions();
            options.delay = delay;
            return collection.PlayRandomClipAt(position, options);
        }
    }
}

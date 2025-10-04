using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace Prototype
{
    [CreateAssetMenu(menuName = "Prototype/Sound Effect Collection")]
    public class SoundEffectCollection : ScriptableObject
    {
        public AudioMixerGroup defaultAudioMixerGroup;
        [SerializeField] List<AudioClip> clips;

        [Header("Additional Settings")]
        [SerializeField, Range(0, 1)] float defaultVolume = .8f;
        [SerializeField, Range(-1, 1)] float additionalGain = 0;
        [SerializeField] float maxPlayInterval = .1f;

        float lastPlayTime;

        protected override void Initialize()
        {
            lastPlayTime = -1;
        }

        public AudioClip GetClip() => clips[0];

        public SoundEffectInstance PlayClip(SoundEffectOptions options = new())
        {
            options.distanceRange = new();
            return PlayClipAt(new(), options);
        }

        public SoundEffectInstance PlayClipAt(Vector3 position, SoundEffectOptions options = new())
        {
            return PlayClipAt(GetClip(), position, options);
        }

        public SoundEffectInstance PlayRandomClip(SoundEffectOptions options = new())
        {
            options.distanceRange = new();
            return PlayRandomClipAt(new(), options);
        }

        public SoundEffectInstance PlayRandomClipAt(Vector3 position, SoundEffectOptions options = new())
        {
            return PlayClipAt(Utils.RandomPick(clips), position, options);
        }

        SoundEffectInstance PlayClipAt(AudioClip clip, Vector3 position, SoundEffectOptions options = new())
        {
            var time = Time.time;

            if (!Mathf.Approximately(maxPlayInterval, 0))
            {
                if (time - lastPlayTime < maxPlayInterval)
                    return null;
            }

            lastPlayTime = time;

            var gameObject = new GameObject(clip.name);
            gameObject.SetActive(false);
            gameObject.transform.position = position;

            if (options.ShouldDestroyOnLoad())
            {
                var reparent = gameObject.AddComponent<Reparent>();
                reparent.parentPath = $"Sound Effects/{name}";
            }

            if (!options.audioMixerGroup)
                options.audioMixerGroup = defaultAudioMixerGroup;

            if (!options.volume.HasValue)
                options.volume = defaultVolume;

            options.volume += additionalGain;

            var soundEffectInstance = gameObject.AddComponent<SoundEffectInstance>();
            soundEffectInstance.clip = clip;
            soundEffectInstance.options = options;

            gameObject.SetActive(true);

            return soundEffectInstance;
        }
    }
}

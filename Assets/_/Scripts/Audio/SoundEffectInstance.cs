using System.Collections;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu("/")]
    public class SoundEffectInstance : MonoBehaviour, IDestroyable
    {
        public AudioClip clip { get; set; }
        public SoundEffectOptions options { get; set; }

        readonly SmoothFloat volume = new(0, 0);

        AudioSource audioSource;

        public bool destroyed { get; private set; }

        float fadeIn => options.GetFade().x;
        float fadeOut => options.GetFade().y;

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>(x =>
            {
                x.playOnAwake = false;
                x.dopplerLevel = 0;
                x.outputAudioMixerGroup = options.audioMixerGroup;
                x.clip = clip;
                x.volume = options.GetVolume();
                x.pitch = options.GetPitch();

                var range = options.GetDistanceRange();

                x.spatialBlend = range.x == 0 && range.y == 0 ? 0 : 1;
                x.rolloffMode = AudioRolloffMode.Linear;
                x.minDistance = range.x;
                x.maxDistance = range.y;
                x.loop = options.GetLoop();
            });

            if (!options.ShouldDestroyOnLoad())
                this.DontDestroy();
        }

        IEnumerator Start()
        {
            var delay = options.GetDelay();
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            audioSource.Play();

            if (fadeIn > 0)
            {
                volume.smoothTime = fadeIn;
                volume.value = 0;
                volume.target = options.GetVolume();
                audioSource.volume = 0;
            }
            else
            {
                volume.value = options.GetVolume();
            }

            if (audioSource.loop)
                yield break;

            yield return new WaitForSecondsRealtime(audioSource.clip.length - fadeOut + .1f);
            gameObject.Destroy();
        }

        void Update()
        {
            volume.Update();
            audioSource.volume = volume;

            if (destroyed && Mathf.Approximately(volume, 0))
                gameObject.Kill();
        }

        public void Destroy()
        {
            if (destroyed)
                return;

            destroyed = true;

            if (fadeOut == 0)
            {
                gameObject.Kill();
                return;
            }

            volume.smoothTime = fadeOut;
            volume.target = 0;
        }
    }
}

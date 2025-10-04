using UnityEngine;
using UnityEngine.Audio;

namespace Prototype
{
    public struct SoundEffectOptions
    {
        public AudioMixerGroup audioMixerGroup;

        public float? volume;
        public float? volumeMultiplier;
        public float GetVolume() => (volume ?? 1) * (volumeMultiplier ?? 1);

        public float? pitch;
        public float GetPitch() => pitch ?? 1;

        public bool? loop;
        public bool GetLoop() => loop ?? false;

        public Vector2? distanceRange;
        public Vector2 GetDistanceRange() => distanceRange ?? new(30, 200);

        public Vector2? fade;
        public Vector2 GetFade() => fade ?? new();

        public bool? destroyOnLoad;
        public bool ShouldDestroyOnLoad() => destroyOnLoad ?? true;

        public float? delay;
        public float GetDelay() => delay ?? 0;
    }
}

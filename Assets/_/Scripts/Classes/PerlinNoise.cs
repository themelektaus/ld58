using System;

using UnityEngine;

namespace Prototype
{
    public class PerlinNoise
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() => lastSeed = 0;

        static int lastSeed;

        float? deltaTime;

        readonly float seed = ++lastSeed + .5f;

        float time;
        int lastFrame;

        float GetTime()
        {
            var time = deltaTime ?? Time.time;
            deltaTime = null;
            return time;
        }

        public PerlinNoise With(float deltaTime)
        {
            this.deltaTime = deltaTime;
            return this;
        }

        float T(Func<float, float> t)
        {
            if (lastFrame != Time.frameCount)
            {
                lastFrame = Time.frameCount;
                if (!deltaTime.HasValue)
                    time = 0;
                time += t is null ? GetTime() : t(GetTime());
            }
            return time;
        }

        public float GetX(Func<float, float> t = null)
            => Mathf.PerlinNoise(seed, T(t));

        public float GetY(Func<float, float> t = null)
            => Mathf.PerlinNoise(T(t), seed);

        public Vector2 GetVector(Func<float, float> t = null)
        => new(GetX(t), GetY(t));

        public Vector2 GetDirection(Func<float, float> t = null)
            => GetPointInsideSquare(t).normalized;

        public Vector2 GetPointInsideSquare(Func<float, float> t = null)
            => (GetVector(t) - new Vector2(.5f, .5f)) * 2;

        public Vector2 GetPointInsideCircle(Func<float, float> t = null)
        {
            var point = GetPointInsideSquare(t);
            if (point.sqrMagnitude > 1)
                point.Normalize();
            return point;
        }
    }
}

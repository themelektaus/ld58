using System;

using UnityEngine;

namespace Prototype.LD58
{
    [ExecuteAlways]
    public class LD58_PerlinNoise : MonoBehaviour
    {
        [Serializable]
        class Axis
        {
            public float speed;
            public Vector2 range;
            public Reference reference;

            public void Update(Func<Func<float, float>, float> perlinNoiseFunc)
            {
                if (speed > 0)
                {
                    if (range.x != range.y)
                    {
                        if (!(reference?.isEmpty ?? false))
                        {
                            reference.Set(
                                Mathf.Lerp(
                                    range.x,
                                    range.y,
                                    perlinNoiseFunc(t => t * speed)
                                )
                            );
                        }
                    }
                }
            }
        }

        [SerializeField] Axis x;
        [SerializeField] Axis y;

        readonly PerlinNoise perlinNoise = new();

        void Update()
        {
            x.Update(perlinNoise.GetX);
            y.Update(perlinNoise.GetY);
        }
    }
}

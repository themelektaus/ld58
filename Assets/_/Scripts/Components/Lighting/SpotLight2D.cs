using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Spot Light (2D)")]
    public class SpotLight2D : Light2D
    {
        public float intensity = 1;
        public float width = 10;
        public float spread = .015f;
        public float spreadFade = 1;

        public float distance = 100;
        public float distanceFadeIn = 2;
        public float distanceFadeOut = 40;

        [SerializeField, Range(0, 1)] float flickerStrength;
        [SerializeField] float flickerSpeed = 6;

        protected override int type => TYPE_SPOT;

        protected override float[] properties
            => new[]
            {
                intensity, _width, spread, spreadFade,
                Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
                Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad),
                0, 0,
                distance, distanceFadeIn, distanceFadeOut
            };

        float _width;

        static float r;
        float _r;

        protected override void Update()
        {
            base.Update();

            if (_r == 0)
                _r = r += (Random.value + 10) * 10;

            if (Mathf.Approximately(flickerStrength, 0))
            {
                _width = width;
                return;
            }

            _width = Mathf.Lerp(
                Mathf.Lerp(0, width, 1 - flickerStrength),
                Mathf.Lerp(width, width * 2, flickerStrength),
                Mathf.PerlinNoise1D((_r + Time.time) * flickerSpeed)
            );
        }
    }
}

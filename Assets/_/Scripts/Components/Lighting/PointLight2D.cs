using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Point Light (2D)")]
    public class PointLight2D : Light2D
    {
        public float radius = 1;
        public float intensity = 1;
        [SerializeField] float smoothness = 1;
        [SerializeField] bool invert;
        
        [SerializeField, Range(0, 1)] float flickerStrength;
        [SerializeField] float flickerSpeed = 6;

        protected override int type => TYPE_POINT;

        protected override float[] properties
            => new[] { _radius, intensity, smoothness, invert ? -1 : 1 };

        float _radius;

        static float r;
        float _r;

        protected override void Update()
        {
            base.Update();

            if (_r == 0)
                _r = r += (Random.value + 10) * 10;

            if (Mathf.Approximately(flickerStrength, 0))
            {
                _radius = radius;
                return;
            }

            _radius = Mathf.Lerp(
                Mathf.Lerp(0, radius, 1 - flickerStrength),
                Mathf.Lerp(radius, radius * 2, flickerStrength),
                Mathf.PerlinNoise1D((_r + Time.time) * flickerSpeed)
            );
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Global Light (2D)")]
    public class GlobalLight2D : Light2D
    {
        protected override int type => TYPE_GLOBAL;

        protected override float[] properties
        {
            get
            {
                var p = transform.position;
                var r = lightDirection * Mathf.Deg2Rad;
                var d = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
                return new[] {
                    exposure, saturation, d.x, d.y,
                    lightPower.x, lightPower.y, lightPower.z, lightDirectionStrength,
                    p.x + bounds.xMin, p.y + bounds.yMin,
                    p.x + bounds.xMax, p.y + bounds.yMax
                };
            }
        }

        [Range(-.5f, 9.5f)] public float exposure;
        [Range(-1, 4)] public float saturation;

        public Vector3 lightPower = Vector3.one;
        [Range(0, 360)] public float lightDirection;
        [Range(0, 5)] public float lightDirectionStrength;

        [SerializeField] Rect bounds = new();
    }
}

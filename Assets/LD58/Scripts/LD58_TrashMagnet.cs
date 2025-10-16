using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_TrashMagnet : MonoBehaviour
    {
        [SerializeField] Vector2 radius = new(1.5f, 7.5f);

        [SerializeField, Range(-1000, 1000)] float force = -200;
        [SerializeField] InterpolationCurve.InterpolationCurve forceFactorCurve;

        [SerializeField, Range(0, 1)] float massIgnorance = .5f;

        void FixedUpdate()
        {
            if (Mathf.Approximately(force, 0) || !forceFactorCurve)
            {
                return;
            }

            var position = (Vector2) transform.position;

            foreach (var trash in LD58_Trash.instances)
            {
                var direction = trash.body.position - position;

                var sqrMagnitude = direction.sqrMagnitude;
                if (sqrMagnitude < radius.x * radius.x || sqrMagnitude > radius.y * radius.y)
                {
                    continue;
                }

                var t = Mathf.InverseLerp(radius.x, radius.y, direction.magnitude);
                var force = this.force * forceFactorCurve.Evaluate(t) * Mathf.Lerp(1, trash.body.mass, massIgnorance);
                trash.body.AddForce(direction.normalized * force);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.greenYellow;
            Gizmos.DrawWireSphere(transform.position, radius.x);
            Gizmos.DrawWireSphere(transform.position, radius.y);
        }
    }
}

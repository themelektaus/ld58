using UnityEngine;

namespace Prototype.LD58
{
    [RequireComponent(typeof(LineRenderer))]
    public class LD58_RoboArmCurve : MonoBehaviour
    {
        [SerializeField] Transform startPoint;
        [SerializeField] Transform endPoint;

        [SerializeField][Range(2, 50)] int curveResolution = 10;

        [Range(-10, 10)] public float curveHeight = 1;

        LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = curveResolution;
            lineRenderer.useWorldSpace = true;
        }

        void Update()
        {
            if (!startPoint || !endPoint)
            {
                return;
            }

            Vector3 p0 = startPoint.position;
            p0.z--;

            Vector3 p2 = endPoint.position;
            p2.z--;

            Vector3 p1;

            Vector3 midPoint = (p0 + p2) / 2f;
            Vector3 direction = (p2 - p0).normalized;
            Vector3 perpendicular = new(-direction.y, direction.x, 0f);
            p1 = midPoint + perpendicular * curveHeight;

            for (int i = 0; i < curveResolution; i++)
            {
                float t = i / (float) (curveResolution - 1);
                Vector3 point = CalculateQuadraticBezierPoint(t, p0, p1, p2);
                lineRenderer.SetPosition(i, point);
            }
        }

        static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 point = uu * p0; // (1-t)^2 * P0
            point += 2 * u * t * p1; // 2(1-t)t * P1
            point += tt * p2;        // t^2 * P2

            return point;
        }
    }
}

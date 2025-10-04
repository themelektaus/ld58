using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SmoothCurveConnector : MonoBehaviour
{
    [Header("Verbindungspunkte")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Kurven-Einstellungen")]
    [Range(10, 100)]
    public int curveResolution = 30; // Anzahl der generierten Punkte

    [Range(-5, 5)]
    public float curveHeight = 1f; // Höhe der Kurve (für Bogen)

    [Header("Bezier-Kontrollpunkt (Optional)")]
    public Transform controlPoint; // Falls null, wird automatisch berechnet

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = curveResolution;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (startPoint && endPoint)
        {
            DrawCurve();
        }
    }

    void DrawCurve()
    {
        Vector3 p0 = startPoint.position;
        p0.z--;

        Vector3 p2 = endPoint.position;
        p2.z--;

        Vector3 p1;

        // Kontrollpunkt berechnen oder verwenden
        if (controlPoint != null)
        {
            p1 = controlPoint.position;
        }
        else
        {
            // Automatischer Kontrollpunkt in der Mitte, nach oben versetzt
            Vector3 midPoint = (p0 + p2) / 2f;
            Vector3 direction = (p2 - p0).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            p1 = midPoint + perpendicular * curveHeight;
        }

        // Quadratische Bezier-Kurve generieren
        for (int i = 0; i < curveResolution; i++)
        {
            float t = i / (float) (curveResolution - 1);
            Vector3 point = CalculateQuadraticBezierPoint(t, p0, p1, p2);
            lineRenderer.SetPosition(i, point);
        }
    }

    // Quadratische Bezier-Formel
    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
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

using Prototype;
using Prototype.LD58;
using System.Linq;
using UnityEngine;

public class LD58_TrashRunAway : MonoBehaviour
{
    [SerializeField] Rigidbody2D body;
    [SerializeField] LD58_RoboArm roboArm;
    [SerializeField] Vector2 radius = new(5, 10);
    [SerializeField] Vector2 idleForce = new(2, 4);
    [SerializeField] Vector2 force = new(20, 65);
    [SerializeField] Vector2 forceInterval = new(.2f, .4f);

    [SerializeField, ReadOnly] bool ready;
    [SerializeField, ReadOnly] Vector2 sourcePosition;
    [SerializeField, ReadOnly] float lastForce;

    float forceTimer = 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!roboArm)
        {
            roboArm = this.EnumerateSceneObjectsByType<LD58_RoboArm>().FirstOrDefault();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    void Awake()
    {
        ready = false;

        this.WaitForFrames(10).Start(() =>
        {
            ready = true;
            sourcePosition = body.position;
        });
    }

    void FixedUpdate()
    {
        if (!ready)
        {
            return;
        }

        if (forceTimer > 0)
        {
            forceTimer -= Time.fixedDeltaTime;
            return;
        }

        var sourceDistance = (sourcePosition - body.position).magnitude;
        var t = Mathf.Clamp01(sourceDistance / (radius.y - radius.x));

        var direction = body.position - roboArm.body.position;
        if (direction.sqrMagnitude < radius.x * radius.x)
        {
            lastForce = Mathf.Lerp(force.x, force.y, t);
        }
        else
        {
            lastForce = Mathf.Lerp(idleForce.x, idleForce.y, t);
        }

        body.AddForce(((sourcePosition + Random.insideUnitCircle * radius) - body.position).normalized * lastForce * body.mass, ForceMode2D.Impulse);
        forceTimer = Utils.RandomRange(forceInterval);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(body.position, radius.x);
        Gizmos.DrawWireSphere(body.position, radius.y);
    }
}

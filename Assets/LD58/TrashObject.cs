using Prototype;
using System.Linq;
using UnityEngine;

public class TrashObject : MonoBehaviour
{
    public ObjectQuery reedAreaQuery;
    public float reedMultiplicatorAdditive = 1;

    public Rigidbody2D body;
    public Collider2D bodyCollider;

    public enum Type { Can, Bottle, Tire }
    public Type type;
    public RoboCursor currentGrabber { get; set; }

    public float ready { get; private set; }

    float linearDamping;

    void Awake()
    {
        linearDamping = body.linearDamping;
    }

    void Update()
    {
        ready -= Time.deltaTime;

        var dampingMultiplicator = 1f;

        var reedAreas = reedAreaQuery.FindComponents<Collider2D>();
        var overlaps = Physics2D.OverlapPointAll(body.position);

        foreach (var reedArea in reedAreas)
        {
            if (overlaps.Any(x => x == reedArea))
            {
                dampingMultiplicator += reedMultiplicatorAdditive;
                break;
            }
        }

        body.linearDamping = linearDamping * dampingMultiplicator;
    }
}

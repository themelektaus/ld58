using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Trash : MonoBehaviour
    {
        public static HashSet<LD58_Trash> instances { get; } = new();

        public List<Collider2D> reedAreas;
        public float reedMultiplicatorAdditive = 1;

        public Rigidbody2D body;
        public Collider2D bodyCollider;

        public LD58_TrashInfo trashInfo;

        public SoundEffect grapSound;

        public LD58_RoboArm currentGrabber { get; set; }

        public float ready { get; private set; }

        float linearDamping;

        void Awake()
        {
            instances.Add(this);
            linearDamping = body.linearDamping;
        }

        void OnValidate()
        {
            if (reedAreas is null || reedAreas.Count == 0)
            {
                reedAreas = this.EnumerateSceneObjectsByType<Collider2D>()
                    .Where(x => x.gameObject.name == "Reed Area")
                    .ToList();
            }
        }

        void OnDestroy()
        {
            instances.Remove(this);
        }

        void Update()
        {
            ready -= Time.deltaTime;

            var dampingMultiplicator = 1f;

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
}

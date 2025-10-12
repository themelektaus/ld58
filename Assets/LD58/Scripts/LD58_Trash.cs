using System.Linq;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Trash : MonoBehaviour
    {
        public static int count { get; private set; }

        public ObjectQuery reedAreaQuery;
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
            count++;
            linearDamping = body.linearDamping;
        }

        void OnDestroy()
        {
            count--;
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
}

using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Switch (2D)")]
    public class Switch2D : MonoBehaviour
    {
        public bool active { get; private set; }
        public float activity
        {
            get
            {
                if (!active)
                    return 0;

                if (stayTime <= 0)
                    return 1;

                return time / stayTime;
            }
        }

        [SerializeField] ObjectQuery requirements;
        [SerializeField] int minimumColliders = 1;
        [SerializeField] float stayTime = -1;

        float time;

        readonly List<Collider2D> colliders = new();

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (requirements && !requirements.Match(collision))
                return;

            if (!colliders.Contains(collision))
                colliders.Add(collision);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            colliders.RemoveAll(x => x == collision);
        }

        void Update()
        {
            bool hasMinimumColliders = colliders.Count >= minimumColliders;

            if (stayTime < 0)
            {
                if (!active && hasMinimumColliders)
                    active = true;
                return;
            }

            if (hasMinimumColliders)
            {
                time = stayTime;
                active = true;
                return;
            }

            time = Mathf.Max(0, time - Time.deltaTime);
            active = time > 0;
        }
    }
}

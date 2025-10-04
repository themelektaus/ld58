using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Top-Down Character Controller (2D)")]
    public class TopDownCharacterController2D : MonoBehaviour
    {
        [HideInInspector] public Vector2 move;
        public Vector2 currentMove { get; private set; }

        public enum MoveMode { RawPosition, Position, Velocity, Acceleration }
        public MoveMode moveMode = MoveMode.Velocity;

        public float speed = 5;
        public float acceleration = 70;
        public Vector2 interactionPointOffset = new(.03f, 0);
        public float maxVelocity = -1;

        public Rigidbody2D body { get; private set; }
#if UNITY_EDITOR
        public new Collider2D collider { get; private set; }
#else
        public Collider2D collider { get; private set; }
#endif
        public Vector2 position => body ? body.position : new();

        public class DirectionOverTime
        {
            public Vector2 direction;
            public float lifetime;
            public float weight;

            readonly float timestamp = Time.time;
            public bool elapsed => Time.time >= timestamp + lifetime;
        }
        public readonly List<DirectionOverTime> moveList = new();
        public readonly List<DirectionOverTime> forceList = new();

        Vector2 velocity;

        public Vector2Int direction { get; set; } = Vector2Int.down;

        public bool isLookingUp => direction == Vector2Int.up;
        public bool isLookingDown => direction == Vector2Int.down;
        public bool isLookingLeft => direction == Vector2Int.left;
        public bool isLookingRight => direction == Vector2Int.right;

        public bool isMoving { get; private set; }

        float forceWeight;

        public Vector2? targetPosition { get; private set; }
        public bool hasTargetPosition => targetPosition.HasValue;

        public bool lockDirection { get; set; }

        public Vector2 interactionPoint
        {
            get
            {
                var point = position + interactionPointOffset;

                if (direction.x != 0)
                {
                    point.x += direction.x * .4f;
                    point.y += .1f;
                }
                else
                {
                    point.y += .2f + direction.y * .3f;
                }

                return point;
            }
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            collider = body.GetComponent<Collider2D>();
        }

        void Update()
        {
            UpdateCurrentMove();
            Look(currentMove, force: false);
            UpdateVelocity();
            UpdateMovement();
        }

        void UpdateCurrentMove()
        {
            if (moveList.Count > 0)
            {
                var move = moveList.FirstOrDefault();
                currentMove = move.direction;
                if (move.elapsed)
                    moveList.Remove(move);
            }

            if (moveList.Count > 0)
                return;

            if (hasTargetPosition)
            {
                var direction = targetPosition.Value - position;
                if (direction.sqrMagnitude > 1)
                {
                    currentMove = direction.normalized;
                    return;
                }
                targetPosition = null;
            }

            currentMove = move;
        }

        void UpdateVelocity()
        {
            velocity = currentMove * speed;

            forceWeight = 0;
            foreach (var force in forceList)
            {
                forceWeight += force.weight;
                velocity += force.direction;
            }
            forceList.RemoveAll(x => x.elapsed);

            isMoving = velocity.sqrMagnitude > .1f;
        }

        void UpdateMovement()
        {
            Vector2 bodyVelocity;

            switch (moveMode)
            {
                default:
                    body.position += velocity * Time.deltaTime;
                    bodyVelocity = Vector2.Lerp(body.linearVelocity, velocity, forceWeight);
                    break;

                case MoveMode.Velocity:
                    bodyVelocity = velocity;
                    break;

                case MoveMode.Acceleration:
                    var maxDistanceDelta = acceleration * Time.deltaTime;
                    bodyVelocity = Vector2.Lerp(
                        Vector2.MoveTowards(body.linearVelocity, velocity, maxDistanceDelta),
                        velocity,
                        forceWeight
                    );
                    break;
            }

            if (maxVelocity >= 0)
                bodyVelocity = bodyVelocity.normalized * Mathf.Min(bodyVelocity.magnitude, maxVelocity);

            body.linearVelocity = bodyVelocity;
        }

        public void ResetVelocity()
        {
            move = new();
            currentMove = new();
            velocity = new();
            body.linearVelocity = new();
        }

        public void ReduceVelocity(float strength)
        {
            var v = body.linearVelocity;
            v /= strength;
            body.linearVelocity = v;
            velocity = v;
        }

        void OnDrawGizmosSelected()
        {
            if (!(body = body ? body : GetComponent<Rigidbody2D>()))
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(interactionPoint, .1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(position, position + (move * 6));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + (currentMove * 8));
        }

        public void Look(Vector2 direction, bool force)
        {
            if (lockDirection)
                return;

            var directionAbsolute = Utils.Abs(direction);

            if (!force)
            {
                if (Utils.Approximately(directionAbsolute.x, directionAbsolute.y, .01f))
                {
                    var x = direction.x;
                    var y = direction.y;

                    var dX = this.direction.x;
                    var dY = this.direction.y;

                    if ((y < 0 && dY > 0) || (y > 0 && dY < 0))
                    {
                        this.direction = new(dX, dY * -1);
                        return;
                    }

                    if ((x < 0 && dX > 0) || (x > 0 && dX < 0))
                    {
                        this.direction = new(dX * -1, dY);
                        return;
                    }

                    return;
                }
            }

            if (directionAbsolute.x > directionAbsolute.y)
            {
                this.direction = new(direction.x > 0 ? 1 : -1, 0);
                return;
            }

            if (force || directionAbsolute.x < directionAbsolute.y)
            {
                this.direction = new(0, direction.y > 0 ? 1 : -1);
                return;
            }
        }

        public void LookAt(Vector2 position)
        {
            Look((position - this.position).normalized, force: true);
        }

        public void Goto(Vector2 position)
        {
            targetPosition = position;
        }
    }
}

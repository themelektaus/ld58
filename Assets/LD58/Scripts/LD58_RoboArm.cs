using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_RoboArm : MonoBehaviour
    {
        public LD58_Player player;

        public LD58_RoboHead roboAvatar;

        public Rigidbody2D body;

        [SerializeField] SpriteRenderer sprite1;
        [SerializeField] SpriteRenderer sprite2;

        [SerializeField] LD58_RoboArmCurve arm;
        [SerializeField] Transform armStart;

        SmoothFloat armValue = new(-1, .2f);

        Camera mainCamera => Utils.GetMainCamera(autoCreate: false);

        public class ConnectedTrashObjects
        {
            public LD58_Trash trashObject;
            public DistanceJoint2D joint;
        }

        readonly List<ConnectedTrashObjects> connectedTrashObjects = new();

        void Update()
        {
            body.mass = player.perks.strength / 100f;

            if (player.isPaused)
            {
                UpdateIdle();
                return;
            }

            UpdateArmPosition();

            if (Input.GetMouseButton(0))
            {
                UpdateGrabbing(player.perks.range);
            }
            else
            {
                UpdateIdle();
            }
        }

        public void UpdateArmPosition()
        {
            var left = body.position.x < roboAvatar.transform.position.x;
            armValue.target = left ? -1 : 1;
            armValue.Update();

            var position = armStart.localPosition;
            position.x = armValue * 8 + roboAvatar.transform.position.x;
            armStart.localPosition = position;

            arm.curveHeight = armValue * 4;
        }

        public void UpdateIdle()
        {
            sprite1.enabled = true;
            sprite2.enabled = false;

            foreach (var connectedTrashObject in connectedTrashObjects)
            {
                connectedTrashObject.joint.Destroy();
            }

            connectedTrashObjects.Clear();
        }

        public void UpdateGrabbing(float radius)
        {
            sprite1.enabled = false;
            sprite2.enabled = true;

            if (connectedTrashObjects.Count >= player.perks.more)
            {
                return;
            }

            var overlapObjects = Physics2D.OverlapCircleAll(body.position, radius);

            var trashObjects = FindObjectsByType<LD58_Trash>(FindObjectsSortMode.None)
                .Where(x => x.ready < -.2f)
                .Where(x => !connectedTrashObjects.Any(y => y.trashObject == x))
                .Where(x => overlapObjects.Contains(x.bodyCollider));

            trashObjects = trashObjects.OrderBy(x => (x.body.position - body.position).sqrMagnitude);

            foreach (var trashObject in trashObjects)
            {
                if (connectedTrashObjects.Count > 0)
                {
                    var trashInfo = connectedTrashObjects.First().trashObject.trashInfo;
                    if (trashObject.trashInfo != trashInfo)
                    {
                        continue;
                    }
                }

                var joint = body.gameObject.AddComponent<DistanceJoint2D>();
                joint.connectedBody = trashObject.body;
                joint.autoConfigureDistance = false;
                joint.distance = 1;
                joint.maxDistanceOnly = true;
                joint.connectedAnchor = new(0, .15f);

                trashObject.currentGrabber = this;
                trashObject.grapSound.PlayRandomClipAt(trashObject.body.position);

                var connectedTrashObject = new ConnectedTrashObjects
                {
                    trashObject = trashObject,
                    joint = joint
                };

                connectedTrashObjects.Add(connectedTrashObject);

                if (connectedTrashObjects.Count >= player.perks.more)
                {
                    break;
                }
            }
        }

        public void DropTrashObject(LD58_Trash trashObject)
        {
            foreach (var connectedTrashObject in connectedTrashObjects)
            {
                if (connectedTrashObject.trashObject == trashObject)
                {
                    connectedTrashObject.joint.Destroy();
                }
            }
            connectedTrashObjects.RemoveAll(x => x.trashObject == trashObject);
        }

        void FixedUpdate()
        {
            if (player.isPaused)
            {
                return;
            }

            Vector2 currentPosition = body.position;
            Vector2 targetPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);

            targetPosition.x = Mathf.Clamp(targetPosition.x, -.1f, 1.1f);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -.1f, 1.1f);

            targetPosition = mainCamera.ViewportToWorldPoint(targetPosition);

            float speed;

            if (roboAvatar.sadness > 0)
            {
                targetPosition.y = -17;
                speed = 100;
            }
            else
            {
                speed = player.perks.speed;
            }

            var direction = targetPosition - currentPosition;
            direction = Mathf.Min(1, direction.magnitude) * direction.normalized;

            body.MovePosition(currentPosition + direction * (speed * Time.fixedDeltaTime));
        }
    }
}

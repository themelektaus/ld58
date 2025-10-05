using Prototype;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Global = Prototype.LD58.LD58_Global;

public class RoboCursor : MonoBehaviour
{
    public RoboAvatar roboAvatar;

    [SerializeField] Rigidbody2D body;
    [SerializeField] float grabPullDistance = 1;

    [SerializeField] SpriteRenderer sprite1;
    [SerializeField] SpriteRenderer sprite2;

    [SerializeField] SmoothCurveConnector arm;
    [SerializeField] Transform armStart;

    SmoothFloat armValue = new(-1, .2f);

    Camera mainCamera => Utils.GetMainCamera(autoCreate: false);

    Global.Data.Upgrade upgrade => Global.instance.data.upgrade;

    public class ConnectedTrashObjects
    {
        public TrashObject trashObject;
        public DistanceJoint2D joint;
    }

    readonly List<ConnectedTrashObjects> connectedTrashObjects = new();

    void Update()
    {
        if (Global.instance.IsPaused())
        {
            sprite1.enabled = true;
            sprite2.enabled = false;
            DropAllTrashObjects();
            return;
        }

        UpdateArmPosition();

        body.mass = upgrade.mass;

        var mouseDown = Input.GetMouseButton(0);

        if (mouseDown)
        {
            sprite1.enabled = false;
            sprite2.enabled = true;
            GrabTrashObjects(upgrade.radius);
        }
        else
        {
            sprite1.enabled = true;
            sprite2.enabled = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            DropAllTrashObjects();
        }
    }

    void UpdateArmPosition()
    {
        var mousePoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var left = mousePoint.x < 0;
        armValue.target = left ? -1 : 1;
        armValue.Update();

        var position = armStart.position;
        position.x = armValue * 12;
        armStart.position = position;

        arm.curveHeight = armValue * 4;
    }

    void GrabTrashObjects(float radius)
    {
        if (connectedTrashObjects.Count >= upgrade.maxObjects)
        {
            return;
        }

        var overlapObjects = Physics2D.OverlapCircleAll(body.position, radius);

        var trashObjects = FindObjectsByType<TrashObject>(FindObjectsSortMode.None)
            .Where(x => x.ready < -.2f)
            .Where(x => !connectedTrashObjects.Any(y => y.trashObject == x))
            .Where(x => overlapObjects.Contains(x.bodyCollider));

        trashObjects = trashObjects.OrderBy(x => (x.body.position - body.position).sqrMagnitude);

        foreach (var trashObject in trashObjects)
        {
            if (connectedTrashObjects.Count > 0)
            {
                var type = connectedTrashObjects.First().trashObject.type;
                if (trashObject.type != type)
                {
                    continue;
                }
            }

            var joint = body.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = trashObject.body;
            joint.autoConfigureDistance = false;
            joint.distance = grabPullDistance;
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

            if (connectedTrashObjects.Count >= upgrade.maxObjects)
            {
                break;
            }
        }
    }

    public void DropTrashObject(TrashObject trashObject)
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

    void DropAllTrashObjects()
    {
        foreach (var connectedTrashObject in connectedTrashObjects)
        {
            connectedTrashObject.joint.Destroy();
        }

        connectedTrashObjects.Clear();
    }

    void FixedUpdate()
    {
        if (Global.instance.IsPaused())
        {
            return;
        }

        Vector2 currentPosition = body.position;
        Vector2 targetPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        float speed;

        if (roboAvatar.sadness > 0)
        {
            targetPosition.y = -17;
            speed = 100;
        }
        else
        {
            speed = upgrade.speed;
        }

        var direction = targetPosition - currentPosition;
        direction = Mathf.Min(1, direction.magnitude) * direction.normalized;

        body.MovePosition(currentPosition + direction * (speed * Time.fixedDeltaTime));
    }
}

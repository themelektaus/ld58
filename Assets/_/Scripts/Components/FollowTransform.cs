using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Follow Transform")]
    public class FollowTransform : MonoBehaviour
    {
        public enum UpdateMode
        {
            None,
            Update,
            FixedUpdate,
            LateUpdate
        }

        [SerializeField] Object target;

        [Header("Position")]
        [SerializeField] UpdateMode positionUpdateMode;
        [SerializeField, ReadOnlyInPlayMode] float positionSmoothTime;
        [SerializeField] Vector3 positionOffset;

        [Header("Rotation")]
        [SerializeField] UpdateMode rotationUpdateMode;
        [SerializeField, ReadOnlyInPlayMode] float rotationSmoothTime;

        public SmoothTransformPosition transformPosition { get; private set; }
        public SmoothTransformRotation transformRotation { get; private set; }

        void Awake()
        {
            transformPosition = new(transform, positionSmoothTime);
            transformRotation = new(transform, rotationSmoothTime);
        }

        void Update()
        {
            if (!target)
                return;

            if (positionUpdateMode == UpdateMode.Update)
                UpdatePosition();

            if (rotationUpdateMode == UpdateMode.Update)
                UpdateRotation();
        }

        void FixedUpdate()
        {
            if (!target)
                return;

            if (positionUpdateMode == UpdateMode.FixedUpdate)
                UpdatePosition();

            if (rotationUpdateMode == UpdateMode.FixedUpdate)
                UpdateRotation();
        }

        void LateUpdate()
        {
            if (!target)
                return;

            if (positionUpdateMode == UpdateMode.LateUpdate)
                UpdatePosition();

            if (rotationUpdateMode == UpdateMode.LateUpdate)
                UpdateRotation();
        }

        public void UpdatePosition()
        {
            if (!transformPosition)
                return;

            var target = GetTargetPosition();
            if (target.HasValue)
                transformPosition.target = target.Value;

            transformPosition.Update();
        }

        public void UpdateRotation()
        {
            if (!transformRotation)
                return;

            var target = GetTargetRotation();
            if (target.HasValue)
                transformRotation.target = target.Value;

            transformRotation.Update();
        }

        Vector3? GetTargetPosition()
        {
            if (!target)
                return null;

            var transform = target.GetTransform();
            if (!transform)
                return null;

            return transform.position + positionOffset;
        }

        Quaternion? GetTargetRotation()
        {
            if (!target)
                return null;

            var transform = target.GetTransform();
            if (!transform)
                return null;

            return target.GetTransform().rotation;
        }

        public void UpdateTarget(Transform target)
        {
            if (this.target == target)
                return;

            this.target = target;

            if (!target)
                return;

            if (transformPosition)
                transformPosition.value = GetTargetPosition().Value;

            if (transformRotation)
                transformRotation.value = GetTargetRotation().Value;
        }
    }
}

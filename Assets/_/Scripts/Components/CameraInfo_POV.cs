#pragma warning disable CS0618

using Unity.Cinemachine;

using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CAMERA + "Camera Info (POV)")]
    public class CameraInfo_POV : CameraInfo
    {
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

        [SerializeField] string moveHorizontal = "Horizontal";
        [SerializeField] string moveVertical = "Vertical";
        [SerializeField] float moveSpeed = .5f;

        [SerializeField] string inputHorizontal = "Mouse X";
        [SerializeField] string inputVertical = "Mouse Y";

        [SerializeField] Vector2 sensitiviyFactor = new(1.75f, 1.25f);
        [SerializeField] Reference sensitiviy;

        SmoothVector2 moveVelocity;

        FieldInfo field;
        CinemachinePOV pov;
        float internalWeight;

        void Awake()
        {
            moveVelocity = new(new(), .2f);
        }

        bool Init()
        {
            if (field is null)
                field = virtualCameraBase.GetType().GetField("m_ComponentPipeline", FLAGS);

            if (!pov)
            {
                var components = field.GetValue(virtualCameraBase) as CinemachineComponentBase[];
                if (components is not null)
                    pov = components.FirstOrDefault(x => x is CinemachinePOV) as CinemachinePOV;
            }

            return pov;
        }

        void OnEnable()
        {
            internalWeight = 0;
        }

        void Update()
        {
            if (!Init())
                return;

            if (Cursor.lockState == CursorLockMode.None)
            {
                internalWeight = 0;
                ResetValues();
                return;
            }

            internalWeight = Mathf.Min(internalWeight + Time.deltaTime, 1);

            pov.m_HorizontalAxis.m_InputAxisName = inputHorizontal;
            pov.m_VerticalAxis.m_InputAxisName = inputVertical;

            var speed = sensitiviy.Get(1f) * (sensitiviyFactor * internalWeight);
            pov.m_HorizontalAxis.m_MaxSpeed = speed.x;
            pov.m_VerticalAxis.m_MaxSpeed = speed.y;

            var x = Input.GetAxisRaw(moveHorizontal);
            var y = Input.GetAxisRaw(moveVertical);
            moveVelocity.target = Vector2.ClampMagnitude(new(x, y), moveSpeed * internalWeight);
            moveVelocity.Update();

            var cameraTransform = Utils.GetMainCamera(autoCreate: false).transform;
            var position = transform.position;
            var move = moveVelocity.current;
            position += cameraTransform.right * move.x;
            position += cameraTransform.forward.ToXZ().normalized.ToX0Z() * move.y;
            transform.position = position;
        }

        void OnDisable()
        {
            internalWeight = 0;

            if (!Init())
                return;

            ResetValues();
        }

        void ResetValues()
        {
            pov.m_HorizontalAxis.m_InputAxisName = "";
            pov.m_VerticalAxis.m_InputAxisName = "";

            pov.m_HorizontalAxis.m_MaxSpeed = 0;
            pov.m_VerticalAxis.m_MaxSpeed = 0;
        }
    }
}

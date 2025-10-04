using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Camera Crop")]
    public class CameraCrop : MonoBehaviour
    {
        [SerializeField] Vector2 targetAspect = new(16, 9);
        [SerializeField] Color backgroundColor = Color.black;

        Camera _camera;
        Camera backgroundCamera;

        int width;
        int height;

        void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        void OnDisable() => OnToggle();

        void OnToggle()
        {
            _camera.rect = new(0, 0, 1, 1);
            if (backgroundCamera)
            {
                backgroundCamera.gameObject.Destroy();
                backgroundCamera = null;
            }
        }

        void Update()
        {
            if (width == Screen.width && height == Screen.height)
                return;

            OnToggle();

            width = Screen.width;
            height = Screen.height;

            float screenRatio = width / (float) height;
            float targetRatio = targetAspect.x / targetAspect.y;

            if (Mathf.Approximately(screenRatio, targetRatio))
                return;

            float n;

            if (screenRatio < targetRatio)
            {
                n = screenRatio / targetRatio;
                _camera.rect = new(0, (1 - n) / 2, 1, n);
            }
            else if (screenRatio > targetRatio)
            {
                n = targetRatio / screenRatio;
                _camera.rect = new((1 - n) / 2, 0, n, 1);
            }

            var backgroundCamera = new GameObject("Background Camera");
            backgroundCamera.transform.parent = _camera.transform;

            this.backgroundCamera = backgroundCamera.AddComponent<Camera>();
            this.backgroundCamera.depth = _camera.depth - 1;
            this.backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
            this.backgroundCamera.backgroundColor = backgroundColor;
            this.backgroundCamera.cullingMask = 0;
        }
    }
}

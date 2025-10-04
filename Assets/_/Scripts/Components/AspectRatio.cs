using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Aspect Ratio")]
    public class AspectRatio : MonoBehaviour
    {
        [SerializeField] Vector2 ratio = new(16, 9);

        Camera _camera;
        Camera GetCamera() => _camera = _camera ? _camera : GetComponent<Camera>();

        Sequence updateSequence;
        Sequence.Instance updateSequenceInstance;

        Vector2 screenSize;

        void Awake()
        {
            updateSequence = this.WaitForFrames(3).Then(() =>
            {
                var rect = new Rect(0, 0, 1, 1);
                var targetRatio = ratio.x / ratio.y;
                var screenRatio = screenSize.x / screenSize.y;

                var s = screenRatio / targetRatio;
                if (s < 1)
                {
                    rect.height = s;
                    rect.y = (1 - s) / 2;
                }
                else
                {
                    rect.width = 1 / s;
                    rect.x = (1 - 1 / s) / 2;
                }
                GetCamera().rect = rect;
            });
        }

        void OnPreCull()
        {
            var camera = GetCamera();
            var cameraRect = camera.rect;
            var fullRect = new Rect(0, 0, 1, 1);
            camera.rect = fullRect;
            GL.Clear(true, true, Color.black);
            camera.rect = cameraRect;
        }

        void Update()
        {
            var screenSize = new Vector2(Screen.width, Screen.height);

            if (Utils.Approximately(this.screenSize, screenSize))
                return;

            this.screenSize = screenSize;

            updateSequenceInstance?.Stop();
            updateSequenceInstance = updateSequence.Start();
        }
    }
}

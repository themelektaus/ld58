using UnityEngine;

namespace Prototype
{
    [ExecuteAlways]
    public class ViewportTransform : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] new Object camera;
#else
        [SerializeField] Object camera;
#endif

        [SerializeField] float aspectRatio = 1;
        [SerializeField] Vector3 offset;
        [SerializeField] bool scale;
        [SerializeField] float scaleFactor;

        void Update()
        {
            var offset = this.offset;
            if (aspectRatio > 1)
            {
                offset.x /= aspectRatio;
            }
            else
            {
                offset.y *= aspectRatio;
            }

            var camera = this.camera
                ? this.camera.GetComponent<Camera>()
                : Utils.GetMainCamera(autoCreate: false);

            if (!camera)
            {
                return;
            }

            transform.position = camera.ViewportToWorldPoint(offset)
                - camera.transform.position;

            if (scale)
            {
                transform.localScale = new(camera.aspect * scaleFactor, scaleFactor, scaleFactor);
            }
        }
    }
}

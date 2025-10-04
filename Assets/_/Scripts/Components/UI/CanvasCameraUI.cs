using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(Canvas))]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Canvas Camera (UI)")]
    public class CanvasCameraUI : MonoBehaviour
    {
        [SerializeField] ObjectQuery cameraQuery;

        Canvas canvas;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        void OnEnable()
        {
            canvas.worldCamera = cameraQuery ? cameraQuery.FindComponent<Camera>() : null;
        }
    }
}

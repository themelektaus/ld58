using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Cursor : MonoBehaviour
    {
        void OnEnable()
        {
            Cursor.visible = false;
        }

        void OnDisable()
        {
            Cursor.visible = true;
        }

        void Update()
        {
            var mainCamera = Utils.GetMainCamera(autoCreate: false);
            transform.position = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}

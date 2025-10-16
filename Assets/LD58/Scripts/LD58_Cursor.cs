using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Cursor : MonoBehaviour
    {
        void OnDisable()
        {
            Cursor.visible = true;
        }

        void Update()
        {
            Cursor.visible = false;
            var mainCamera = Utils.GetMainCamera(autoCreate: false);
            transform.position = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}

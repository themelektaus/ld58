using Prototype;
using UnityEngine;

public class GameMouseCursor : MonoBehaviour
{
    void Update()
    {
        Cursor.visible = false;

        var mainCamera = Utils.GetMainCamera(autoCreate: false);
        transform.position = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}

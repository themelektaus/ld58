using UnityEngine;

public class CursorVisibility : MonoBehaviour
{
    [SerializeField] bool visible = true;

    void Update()
    {
        Cursor.visible = visible;
    }
}

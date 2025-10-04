using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Resolution")]
    public class Resolution : MonoBehaviour
    {
        [SerializeField] int width = 1920;
        [SerializeField] int height = 1080;
        [SerializeField] bool fullscreen = true;

        void OnEnable()
        {
            Screen.SetResolution(width, height, fullscreen);
        }
    }
}

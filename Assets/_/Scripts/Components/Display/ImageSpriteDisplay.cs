using UnityEngine;
using UnityEngine.UI;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Image Sprite Display")]
    public class ImageSpriteDisplay : MonoBehaviour
    {
        [SerializeField] Reference value;

        Image image;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        void Update()
        {
            image.sprite = value.Get<Sprite>();
            image.enabled = image.sprite;
        }
    }
}

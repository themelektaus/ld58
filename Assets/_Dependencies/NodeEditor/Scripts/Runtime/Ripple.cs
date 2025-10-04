using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor
{
    public class Ripple : MonoBehaviour
    {
        public float speed;
        public float maxSize;
        public Color startColor;
        public Color transitionColor;

        Image colorImage;

        void Start()
        {
            transform.localScale = Vector3.zero;

            colorImage = GetComponent<Image>();
            colorImage.raycastTarget = false;
            colorImage.color = startColor;
        }

        void Update()
        {
            var t = Time.unscaledDeltaTime * speed;

            transform.localScale = Vector3.Lerp(transform.localScale, new(maxSize, maxSize, maxSize), t);
            colorImage.color = Color.Lerp(colorImage.color, transitionColor, t);

            if (transform.localScale.x >= maxSize * .998f)
            {
                if (transform.parent.childCount == 1)
                    transform.parent.gameObject.SetActive(false);

                Destroy(gameObject);
            }
        }
    }
}
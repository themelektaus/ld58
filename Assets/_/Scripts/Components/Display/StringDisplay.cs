using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "String Display")]
    public class StringDisplay : MonoBehaviour
    {
        [SerializeField] bool autoUpdate = true;

        [SerializeField] protected Reference value;

        protected TMPro.TMP_Text tmp;

        void Awake()
        {
            tmp = GetComponent<TMPro.TMP_Text>();
        }

        void Start()
        {
            if (!autoUpdate)
                UpdateText();
        }

        void Update()
        {
            if (autoUpdate)
                UpdateText();
        }

        protected virtual void UpdateText()
        {
            tmp.text = value.Get(string.Empty);
        }
    }
}

using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_PlayerPerkDisplay : MonoBehaviour
    {
        public LD58_PlayerPerkInfo info;

        [SerializeField] RectTransform icon;
        [SerializeField] RectTransform value;
        [SerializeField] float minWidth = 0;
        [SerializeField] float maxWidth = 0;

        void Start()
        {
            info.uiIcon.Instantiate(parent: icon);
        }

        void Update()
        {
            var size = value.sizeDelta;
            size.x = Mathf.Lerp(minWidth, maxWidth, (float) info.currentLevel / (info.values.Length - 1));
            value.sizeDelta = size;
        }
    }
}

using UnityEngine;
using UnityEngine.Serialization;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Float Display")]
    public class FloatDisplay : MonoBehaviour
    {
        [SerializeField] FloatFormat format;
        [SerializeField, FormerlySerializedAs("value")] Reference value;

        TMPro.TMP_Text tmp;

        void Awake()
        {
            tmp = GetComponent<TMPro.TMP_Text>();
        }

        void Update()
        {
            tmp.text = value.Get<float>().ToFormattedString(format);
        }
    }
}

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Integer Display")]
    public class IntegerDisplay : MonoBehaviour
    {
        [SerializeField] Reference value;

        TMPro.TMP_Text tmp;

        void Awake()
        {
            tmp = GetComponent<TMPro.TMP_Text>();
        }

        void Update()
        {
            tmp.text = value.Get<int>().ToString();
        }
    }
}

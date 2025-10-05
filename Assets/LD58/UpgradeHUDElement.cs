using UnityEngine;

[ExecuteAlways]
public class UpgradeHUDElement : MonoBehaviour
{
    [SerializeField] RectTransform valueTransform;
    [SerializeField] float minWidth = 0;
    [SerializeField] float maxWidth = 0;

    [SerializeField] int maxValue = 1;

    [SerializeField] Prototype.Reference value;

    void Update()
    {
        if (!valueTransform || value is null)
        {
            return;
        }

        var size = valueTransform.sizeDelta;
        size.x = Mathf.Lerp(minWidth, maxWidth, (float) value.Get<int>() / maxValue);
        valueTransform.sizeDelta = size;
    }
}

using System.Collections.Generic;

using UnityEngine;

namespace NodeEditor
{
    public static class Utils
    {
        public static bool Approximately(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x)
                && Mathf.Approximately(a.y, b.y);
        }

        public static void SetRect(RectTransform rectTransform, Vector3 from, Vector3 to)
        {
            var sizeDelta = to - from;
            sizeDelta.x = Mathf.Abs(sizeDelta.x);
            sizeDelta.y = Mathf.Abs(sizeDelta.y);

            rectTransform.position = from + (to - from) / 2;
            rectTransform.sizeDelta = sizeDelta / rectTransform.lossyScale.x;
        }

        public static bool IsInteractable(this Transform transform)
        {
            var canvasGroups = new List<CanvasGroup>();
            var result = true;

            while (transform)
            {
                transform.GetComponents(canvasGroups);
                var ignoreParentGroups = false;

                for (int i = 0, count = canvasGroups.Count; i < count; i++)
                {
                    var canvasGroup = canvasGroups[i];
                    result &= canvasGroup.interactable;
                    ignoreParentGroups |= canvasGroup.ignoreParentGroups || !canvasGroup.interactable;
                }

                if (ignoreParentGroups)
                    break;

                transform = transform.parent;
            }

            return result;
        }
    }
}

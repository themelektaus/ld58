using UnityEngine;

namespace Prototype
{
    public class SmoothCanvasGroupAlpha : SmoothFloat
    {
        public readonly CanvasGroup canvasGroup;

        public SmoothCanvasGroupAlpha(CanvasGroup canvasGroup, float smoothTime) :
            base(
                () => canvasGroup.alpha,
                x =>
                {
                    canvasGroup.alpha = x;
                    canvasGroup.interactable = canvasGroup.alpha == 1;
                    canvasGroup.blocksRaycasts = canvasGroup.interactable;
                },
                smoothTime
            )
        {
            this.canvasGroup = canvasGroup;
        }
    }
}

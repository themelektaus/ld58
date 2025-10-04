using System.Collections;

using UnityEngine;

namespace Prototype
{
    public class LoadingScene : MonoBehaviour
    {
        public static LoadingScene current { get; private set; }

        public Material fadeMaterial;
        public Vector2 fadeSmoothTime = new(.05f, .15f);

        public Animator content;
        public float minimumLoadingTime = .7f;

        public bool ReadyForLoading { get; private set; }
        public bool LoadingDone { private get; set; }
        public bool Exited { get; private set; }

        SmoothFloat fadeWeight;

        void Awake()
        {
            current = this;

            fadeWeight = new(
                () => fadeMaterial.GetFloat("_Weight"),
                x => fadeMaterial.SetFloat("_Weight", x),
                0
            )
            {
                value = 0,
                boost = .3f
            };
        }

        void OnDestroy()
        {
            current = null;
        }

        IEnumerator Start()
        {
            fadeWeight.smoothTime = fadeSmoothTime.x;
            fadeWeight.target = 1;
            while (fadeWeight < 1)
            {
                fadeWeight.Update();
                yield return new WaitForEndOfFrame();
            }

            content.gameObject.SetActive(true);

            yield return new WaitUntil(() => ContentHasState("Visible"));

            ReadyForLoading = true;

            var waitTime = Time.unscaledTime;

            yield return new WaitUntil(() => LoadingDone);

            waitTime += minimumLoadingTime - Time.unscaledTime;
            if (waitTime > 0)
            {
                yield return new WaitForSecondsRealtime(waitTime);
            }

            content.SetTrigger("Hide");

            yield return new WaitUntil(() => ContentHasState("Hidden"));

            fadeWeight.smoothTime = fadeSmoothTime.y;
            fadeWeight.target = 0;
            while (fadeWeight > 0)
            {
                fadeWeight.Update();
                fadeMaterial.SetFloat("_Weight", fadeWeight);
                yield return new WaitForEndOfFrame();
            }

            Exited = true;
        }

        bool ContentHasState(string name)
        {
            return content.GetCurrentAnimatorStateInfo(0).IsName(name);
        }
    }
}

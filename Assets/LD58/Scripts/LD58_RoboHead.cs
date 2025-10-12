using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_RoboHead : MonoBehaviour
    {
        public GameObject face;
        public GameObject faceHappy;
        public GameObject faceSad;

        public float happiness;
        public float sadness;

        [SerializeField] Animator animator;

        void Update()
        {
            happiness = Mathf.Max(0, happiness);
            sadness = Mathf.Max(0, sadness);

            face.SetActive(Mathf.Approximately(happiness, 0) && Mathf.Approximately(sadness, 0));
            faceHappy.SetActive(happiness > 0 && Mathf.Approximately(sadness, 0));
            faceSad.SetActive(sadness > 0);

            animator.SetLayerWeight(1, happiness);
            animator.SetLayerWeight(2, sadness);

            happiness -= Time.deltaTime;
            sadness -= Time.deltaTime;

            happiness = Mathf.Max(0, happiness);
            sadness = Mathf.Max(0, sadness);
        }
    }
}

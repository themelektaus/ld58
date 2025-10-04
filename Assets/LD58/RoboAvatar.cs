using UnityEngine;

public class RoboAvatar : MonoBehaviour
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

        if (happiness > 0 || sadness > 0)
        {
            face.SetActive(false);
            faceHappy.SetActive(happiness > sadness);
            faceSad.SetActive(happiness <= sadness);
        }
        else
        {
            face.SetActive(true);
            faceHappy.SetActive(false);
            faceSad.SetActive(false);
        }

        animator.SetLayerWeight(1, happiness);
        animator.SetLayerWeight(2, sadness);

        happiness -= Time.deltaTime;
        sadness -= Time.deltaTime;

        happiness = Mathf.Max(0, happiness);
        sadness = Mathf.Max(0, sadness);
    }
}

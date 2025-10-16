using Prototype;
using UnityEngine;

public class LD58_VictoryScreen : MonoBehaviour
{
    public int points;

    [SerializeField] RectTransform[] stars;

    Sequence.Instance sequenceInstance;

    void Start()
    {
        for (var i = 0; i < stars.Length; i++)
        {
            stars[i].Find("Filled").gameObject.SetActive(i < points);
            stars[i].localScale = Vector3.zero;
        }

        var sequence = this.CreateSequence();
        foreach (var star in stars)
        {
            var simpleAnimation = star.GetComponent<SimpleAnimation>();
            sequence = sequence.Wait(.1f).Then(simpleAnimation.Play);
        }

        sequenceInstance = sequence.Start();
    }

    void OnDestroy()
    {
        sequenceInstance?.Stop();
    }
}

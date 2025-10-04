using Prototype;
using UnityEngine;

public class LevelUpScreen : MonoBehaviour
{
    [SerializeField] RoboCursor roboCursor;

    public Transform card1Holder;
    public Transform card2Holder;

    public GameObject[] cardPrefabs;

    GameObject card1;
    GameObject card2;

    void OnEnable()
    {
        roboCursor.enabled = false;

        var card1Prefab = Utils.RandomPick(cardPrefabs);
        var card2Prefab = card1Prefab;

        while (card1Prefab == card2Prefab)
        {
            card2Prefab = Utils.RandomPick(cardPrefabs);
        }

        card1 = card1Prefab.Instantiate(card1Holder);
        card2 = card2Prefab.Instantiate(card2Holder);

        card1.AddComponent<OnPointerDown_>(x =>
        {
            x.onLeftClick.AddListener(() =>
            {
                card1.GetComponent<OnInvoke>().Invoke();
                gameObject.SetActive(false);
            });
        });

        card2.AddComponent<OnPointerDown_>(x =>
        {
            x.onLeftClick.AddListener(() =>
            {
                card2.GetComponent<OnInvoke>().Invoke();
                gameObject.SetActive(false);
            });
        });
    }

    void OnDisable()
    {
        card1.Destroy();
        card2.Destroy();

        roboCursor.enabled = true;
    }
}

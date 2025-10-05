using Prototype;
using Prototype.LD58;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpScreen : MonoBehaviour
{
    [SerializeField] RoboCursor roboCursor;
    [SerializeField] SoundEffect sound1;

    public Transform card1Holder;
    public Transform card2Holder;

    public GameObject[] cardPrefabs;

    GameObject card1;
    GameObject card2;

    void OnEnable()
    {
        LD58_Global.instance.Pause();

        var upgrade = LD58_Global.instance.data.upgrade;

        var cardPrefabs = new List<GameObject>();

        if (upgrade.mass.current < upgrade.mass.values.Length - 1)
            cardPrefabs.Add(this.cardPrefabs[0]);

        if (upgrade.speed.current < upgrade.speed.values.Length - 1)
            cardPrefabs.Add(this.cardPrefabs[1]);

        if (upgrade.radius.current < upgrade.radius.values.Length - 1)
            cardPrefabs.Add(this.cardPrefabs[2]);

        if (upgrade.maxObjects.current < upgrade.maxObjects.values.Length - 1)
            cardPrefabs.Add(this.cardPrefabs[3]);

        if (cardPrefabs.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        var card1Prefab = Utils.RandomPick(cardPrefabs);
        GameObject card2Prefab;

        if (cardPrefabs.Count > 1)
        {
            card2Prefab = card1Prefab;
            while (card1Prefab == card2Prefab)
                card2Prefab = Utils.RandomPick(cardPrefabs);
        }
        else
        {
            card2Prefab = null;
        }

        card1 = card1Prefab.Instantiate(card1Holder);

        if (card2Prefab)
            card2 = card2Prefab.Instantiate(card2Holder);

        var clicked = false;

        card1.AddComponent<OnPointerDown_>(x =>
        {
            x.onLeftClick.AddListener(() =>
            {
                if (clicked)
                    return;
                clicked = true;
                sound1.Play();
                card1.GetComponent<OnInvoke>().Invoke();
                card1.GetComponent<SimpleAnimation>().Play();
                this.Wait(.2f).Start(() =>
                {
                    card1.Destroy();
                    if (card2)
                        card2.Destroy();
                    gameObject.SetActive(false);
                });
            });
        });

        if (card2)
            card2.AddComponent<OnPointerDown_>(x =>
            {
                x.onLeftClick.AddListener(() =>
                {
                    if (clicked)
                        return;
                    clicked = true;
                    sound1.Play();
                    card2.GetComponent<OnInvoke>().Invoke();
                    card2.GetComponent<SimpleAnimation>().Play();
                    this.Wait(.2f).Start(() =>
                    {
                        card1.Destroy();
                        card2.Destroy();
                        gameObject.SetActive(false);
                    });
                });
            });
    }

    void OnDisable()
    {
        LD58_Global.instance.Resume();
    }
}

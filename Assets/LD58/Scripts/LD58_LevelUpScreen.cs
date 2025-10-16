using System.Collections.Generic;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_LevelUpScreen : MonoBehaviour
    {
        [SerializeField] LD58_Player player;
        [SerializeField] LD58_RoboArm roboCursor;
        [SerializeField] SoundEffect sound1;

        public Transform card1Holder;
        public Transform card2Holder;

        GameObject card1;
        GameObject card2;

        float time;

        void Update()
        {
            time -= Time.deltaTime;
        }

        void OnEnable()
        {
            time = .7f;

            var perks = new List<LD58_PlayerPerkInfo>();

            foreach (var perk in player.EnumeratePerks())
            {
                if (perk.currentLevel < perk.values.Length - 1)
                {
                    perks.Add(perk);
                }
            }

            if (perks.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            LD58_PlayerPerkInfo info1 = Utils.RandomPick(perks);
            LD58_PlayerPerkInfo info2;

            if (perks.Count > 1)
            {
                info2 = info1;

                while (info1 == info2)
                {
                    info2 = Utils.RandomPick(perks);
                }
            }
            else
            {
                info2 = null;
            }

            card1 = info1.uiCard.Instantiate(card1Holder);

            if (info2)
            {
                card2 = info2.uiCard.Instantiate(card2Holder);
            }

            var clicked = false;

            card1.AddComponent<OnPointerDown_>(x =>
            {
                x.onLeftClick.AddListener(() =>
                {
                    if (time > 0 || clicked)
                    {
                        return;
                    }

                    clicked = true;

                    sound1.Play();

                    info1.LevelUp();
                    card1.GetComponent<SimpleAnimation>().Play();

                    this.Wait(.2f).Start(() =>
                    {
                        card1.Destroy();

                        if (card2)
                        {
                            card2.Destroy();
                        }

                        gameObject.SetActive(false);
                    });
                });
            });

            if (!card2)
            {
                return;
            }

            card2.AddComponent<OnPointerDown_>(x =>
            {
                x.onLeftClick.AddListener(() =>
                {
                    if (time > 0 || clicked)
                    {
                        return;
                    }

                    clicked = true;

                    sound1.Play();

                    info2.LevelUp();
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
    }
}

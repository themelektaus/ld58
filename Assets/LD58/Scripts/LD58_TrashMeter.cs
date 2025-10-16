using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_TrashMeter : MonoBehaviour
    {
        [SerializeField] LD58_Player player;

        [SerializeField] RectTransform indicator;
        [SerializeField] RectTransform indicatorStart;
        [SerializeField] RectTransform indicatorEnd;

        [SerializeField] GameObject victory;
        [SerializeField] GameObject victoryNextButton;
        [SerializeField] GameObject victoryHappy;
        [SerializeField] GameObject victorySad;
        [SerializeField] SoundEffect victorySadSoundEffect;

        [SerializeField] GameObject specialVictory;

        SmoothTransformPosition indicatorPosition;

        [SerializeField] SimpleAnimation pulseAnimation;

        SimpleAnimationValues simpleAnimationValues;
        struct SimpleAnimationValues
        {
            public float speed;
            public float scale;
            public float min;
            public float max;
        }

        void Awake()
        {
            indicatorPosition = new(indicator, .2f)
            {
                value = indicator.position
            };

            simpleAnimationValues = new()
            {
                speed = pulseAnimation.speed,
                scale = transform.localScale.x,
                min = pulseAnimation.min,
                max = pulseAnimation.max
            };
        }

        void Update()
        {
            if (player.isPaused)
            {
                return;
            }

            var t = player.maxTrash > 0 ? (float) LD58_Trash.instances.Count / player.maxTrash : 1;

            indicatorPosition.target = Vector3.Lerp(indicatorEnd.position, indicatorStart.position, t);
            indicatorPosition.Update();

            pulseAnimation.speed = Mathf.Lerp(1, simpleAnimationValues.speed, t);
            pulseAnimation.min = Mathf.Lerp(simpleAnimationValues.scale, simpleAnimationValues.min, t);
            pulseAnimation.max = Mathf.Lerp(simpleAnimationValues.scale, simpleAnimationValues.max, t);

            if (LD58_Trash.instances.Count <= 0)
            {
                var victory = specialVictory ? specialVictory : this.victory;

                if (!victory.activeSelf)
                {
                    var points = 3;

                    if (!player.points.inTime)
                    {
                        points--;
                    }

                    if (!player.points.neverSad)
                    {
                        points--;
                    }

                    LD58_Savegame.Get().SavePoints(gameObject.scene.name, points);

                    if (victory.TryGetComponent(out LD58_VictoryScreen victoryScreen))
                    {
                        victoryScreen.points = points;
                    }

                    victory.SetActive(true);
                    if (victoryNextButton)
                    {
                        victoryNextButton.SetActive(true);
                    }
                    victoryHappy.SetActive(true);
                    victorySad.SetActive(false);
                }
            }
            else if (LD58_Trash.instances.Count > player.maxTrash)
            {
                if (!victory.activeSelf)
                {
                    if (victory.TryGetComponent(out LD58_VictoryScreen victoryScreen))
                    {
                        victoryScreen.points = 0;
                    }

                    victory.SetActive(true);
                    if (victoryNextButton)
                    {
                        victoryNextButton.SetActive(false);
                    }
                    victoryHappy.SetActive(false);
                    victorySad.SetActive(true);
                    victorySadSoundEffect.Play();
                }
            }
        }
    }
}

using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_TrashMeter : MonoBehaviour
    {
        [SerializeField] LD58_Player player;

        [SerializeField] RectTransform indicator;
        [SerializeField] RectTransform indicatorStart;
        [SerializeField] RectTransform indicatorEnd;

        public int maxValue = 20;

        [SerializeField] GameObject gameOver;
        [SerializeField] SoundEffect gameOverSoundEffect;
        [SerializeField] GameObject victory;

        void Update()
        {
            if (player.isPaused)
            {
                return;
            }

            indicator.position = Vector3.Lerp(indicatorEnd.position, indicatorStart.position, (float) LD58_Trash.count / maxValue);

            if (LD58_Trash.count <= 0)
            {
                victory.SetActive(true);
            }
            else if (LD58_Trash.count > maxValue)
            {
                if (!gameOver.activeSelf)
                {
                    gameOver.SetActive(true);
                    gameOverSoundEffect.Play();
                }
            }
        }
    }
}

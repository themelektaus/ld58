using Prototype;
using Prototype.LD58;
using UnityEngine;

public class TrashinessUI : MonoBehaviour
{
    [SerializeField] RectTransform indicator;
    [SerializeField] RectTransform indicatorStart;
    [SerializeField] RectTransform indicatorEnd;

    [SerializeField] int maxValue = 20;
    [SerializeField] GameObject gameOver;
    [SerializeField] SoundEffect gameOverSoundEffect;
    [SerializeField] GameObject victory;

    void Update()
    {
        var value = FindObjectsByType<TrashObject>(FindObjectsSortMode.None).Length;

        indicator.position = Vector3.Lerp(indicatorEnd.position, indicatorStart.position, (float) value / maxValue);

        if (value <= 1)
        {
            LD58_Global.instance.Pause();
            victory.SetActive(true);
        }
        else if (value > maxValue)
        {
            LD58_Global.instance.Pause();
            if (!gameOver.activeSelf)
            {
                gameOver.SetActive(true);
                gameOverSoundEffect.Play();
            }
        }
    }
}

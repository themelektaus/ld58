using Prototype;
using Prototype.LD58;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class LD58_LevelButton : MonoBehaviour
{
    [SerializeField] string text1;
    [SerializeField] string text2;

    [SerializeField] LD58_VictoryScreen stars;
    [SerializeField] TMPro.TMP_Text uiText1;
    [SerializeField] TMPro.TMP_Text uiText2;

    [SerializeField] LD58_LevelButton previous;
    [ReadOnly] public int points;
    [ReadOnly, SerializeField] ButtonUI button;
    [ReadOnly, SerializeField] CanvasGroup canvasGroup;

    void OnValidate()
    {
        button = GetComponent<ButtonUI>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            var name = GetComponent<GotoScene>().sceneName;
            if (!string.IsNullOrEmpty(name))
            {
                var level = LD58_Savegame.Get().levels.FirstOrDefault(x => x.name == name);
                points = level is null ? 0 : level.points;
            }
        }
    }

    void Update()
    {
        button.disabled = previous && Mathf.Approximately(previous.points, 0);
        canvasGroup.enabled = button.disabled;

        if (uiText1)
            uiText1.text = text1;

        if (uiText2)
            uiText2.text = text2;

        if (Application.isPlaying)
        {
            if (stars)
            {
                stars.enabled = true;
                stars.points = points;
            }
        }
    }
}

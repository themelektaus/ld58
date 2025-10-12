using UnityEngine;

namespace Prototype.LD58
{
    [CreateAssetMenu]
    public class LD58_PlayerPerkInfo : UnityEngine.ScriptableObject
    {
        public static readonly Observable<LevelUpMessage> onLevelUp = new();

        public class LevelUpMessage : Message
        {
            public LD58_PlayerPerkInfo info;
        }

        public GameObject uiIcon;
        public GameObject uiCard;

        public int currentLevel { get; private set; }

        public int[] values;

        public static implicit operator int(LD58_PlayerPerkInfo info)
        {
            return info.values.Length > 0
                ? info.values[Mathf.Clamp(info.currentLevel, 0, info.values.Length - 1)]
                : 0;
        }

        public void ResetCurrentLevel()
        {
            currentLevel = 0;
            onLevelUp.Notify(new() { info = this });
        }

        public void LevelUp()
        {
            currentLevel++;
            onLevelUp.Notify(new() { info = this });
        }
    }
}

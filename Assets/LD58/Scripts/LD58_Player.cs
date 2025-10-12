using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.LD58
{
    public class LD58_Player : MonoBehaviour
    {
        public static readonly Observable<PauseChangeMessage> onPauseChange = new();

        public class PauseChangeMessage : Message
        {
            public bool isPaused;
        }

        [SerializeField] GameObject welcomeScreen;
        [SerializeField] LD58_LevelUpScreen levelUpScreen;
        [SerializeField, ReadOnlyInPlayMode] ButtonUI nextLevelButton;

        [SerializeField] int pause;
        [SerializeField] int maxTrash = 60;

        [SerializeField, ReadOnly] string nextLevel;

        public int currentInventoryLevel;
        [SerializeField] int[] levelValues;

        public Perks perks;

        [Serializable]
        public class Perks
        {
            public LD58_PlayerPerkInfo more;
            public LD58_PlayerPerkInfo range;
            public LD58_PlayerPerkInfo speed;
            public LD58_PlayerPerkInfo strength;
        }

        public IEnumerable<LD58_PlayerPerkInfo> EnumeratePerks()
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;

            foreach (var perk in typeof(Perks).GetFields(flags))
            {
                yield return perk.GetValue(perks) as LD58_PlayerPerkInfo;
            }
        }

        [Serializable]
        public class Slot
        {
            public LD58_TrashInfo trashInfo;
            public int amount;
            public int GetValue()
            {
                return amount * trashInfo.value;
            }
        }

        public List<Slot> inventory;

        void Awake()
        {
            if (welcomeScreen)
            {
                welcomeScreen.SetActive(true);
            }

            GetComponentInChildren<LD58_TrashMeter>().maxValue = maxTrash;

            foreach (var perk in EnumeratePerks())
            {
                perk.ResetCurrentLevel();
            }

            var currentLevelScene = (LevelScene) gameObject.scene.name;
            
            if (currentLevelScene.name is not null)
            {
                var levelScenes = GetLevelScenes().ToList();

                var index = levelScenes.FindIndex(x => x.level == currentLevelScene.level);
                if (index > 0 && index < levelScenes.Count - 1)
                {
                    nextLevel = levelScenes[index + 1].name;
                    nextLevelButton.GetComponent<GotoScene>().sceneName = nextLevel;
                    return;
                }
            }

            nextLevelButton.gameObject.Kill();
            nextLevelButton = null;
        }

        struct LevelScene
        {
            public string name;
            public int level;

            public static explicit operator LevelScene(string sceneName)
            {
                var match = Regex.Match(sceneName, "([0-9]+)");
                return match.Success
                    ? new() { name = sceneName, level = int.Parse(match.Groups[1].Value) }
                    : default;
            }
        }

        IEnumerable<LevelScene> GetLevelScenes()
        {
            return Enumerable.Range(0, SceneManager.sceneCountInBuildSettings)
                .Select(i => SceneUtility.GetScenePathByBuildIndex(i).Split('/').LastOrDefault())
                .Where(x => x?.EndsWith(".unity") ?? false)
                .Select(x => (LevelScene) x[..^6])
                .Where(x => x.name is not null)
                .OrderBy(x => x.level);
        }

        void Update()
        {
            if (isPaused)
            {
                return;
            }

            var inventoryLevel = GetInventoryLevel();

            if (currentInventoryLevel >= inventoryLevel)
            {
                return;
            }

            currentInventoryLevel++;

            levelUpScreen.gameObject.SetActive(true);
        }

        public void Retry()
        {
            var activeSceneName = SceneManager.GetActiveScene().name;
            LD58_Global.GetSingletonInstance(x => x.gameStateMachine).Trigger("Switch Level");
            SceneSwitcher.GotoScene(activeSceneName, activeSceneName, 1);
        }

        public bool isPaused
        {
            get
            {
                if (pause < 0)
                {
                    Debug.LogWarning("Negative pause");
                    return false;
                }

                return pause > 0;
            }
        }

        public void Pause()
        {
            pause++;
            onPauseChange.Notify(new() { isPaused = isPaused });
        }

        public void Resume()
        {
            pause--;
            onPauseChange.Notify(new() { isPaused = isPaused });
        }

        Slot GetOrAddSlot(LD58_TrashInfo trashInfo)
        {
            var slot = inventory.Find(x => x.trashInfo == trashInfo);
            if (slot is null)
            {
                slot = new() { trashInfo = trashInfo };
                inventory.Add(slot);
            }
            return slot;
        }

        public void Collect(LD58_TrashInfo trashInfo)
        {
            GetOrAddSlot(trashInfo).amount++;
        }

        public int GetInventoryLevel()
        {
            var inventoryValue = inventory.Sum(x => x.GetValue());

            var level = -1;
            foreach (var levelValue in levelValues)
            {
                if (levelValue <= inventoryValue)
                {
                    level++;
                }
            }

            return level;
        }
    }
}

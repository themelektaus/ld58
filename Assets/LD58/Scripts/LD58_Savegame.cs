using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_Savegame
    {
        static LD58_Savegame current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            current = null;
        }

        public List<Level> levels = new();

        [Serializable]
        public class Level
        {
            public string name;
            public int points;
        }

        public static LD58_Savegame Get()
        {
            if (current is null)
            {
                Load();
            }

            return current;
        }

        static void Load()
        {
            try
            {
                var json = PlayerPrefs.GetString(nameof(LD58_Savegame), null);
                current = JsonUtility.FromJson<LD58_Savegame>(json) ?? new();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                current = new();
            }
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(nameof(LD58_Savegame), json);
            PlayerPrefs.Save();
        }

        public void SavePoints(string name, int points)
        {
            var level = levels.FirstOrDefault(x => x.name == name);

            if (level is null)
            {
                level = new Level { name = name, points = points };
                levels.Add(level);
                Save();
                return;
            }

            if (level.points < points)
            {
                level.points = points;
                Save();
            }
        }

        public void Delete()
        {
            current = null;
            PlayerPrefs.DeleteKey(nameof(LD58_Savegame));
            PlayerPrefs.Save();
        }
    }
}

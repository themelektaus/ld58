using Prototype;
using Prototype.LD58;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LD58_Level : MonoBehaviour
{
    [SerializeField] LD58_Player player;
    [SerializeField] int destroyOnTrashCountBelowThan = 8;

    [SerializeField] List<Level> levels;

    [Serializable]
    public class Level
    {
        public int min;
        public int max;
        public List<GameObject> objects;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!player)
        {
            player = this.EnumerateSceneObjectsByType<LD58_Player>().FirstOrDefault();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    void Update()
    {
        if (player.isPaused)
        {
            foreach (var @object in levels.SelectMany(x => x.objects))
            {
                @object.SetActive(false);
            }
            return;
        }

        if (LD58_Trash.instances.Count <= destroyOnTrashCountBelowThan)
        {
            gameObject.Destroy();
            return;
        }

        var disabledObjects = levels.SelectMany(x => x.objects).ToList();
        var enabledObjects = new List<GameObject>();

        foreach (var level in levels)
        {
            if (level.min <= player.currentInventoryLevel && player.currentInventoryLevel <= level.max)
            {
                foreach (var @object in level.objects)
                {
                    disabledObjects.Remove(@object);
                    enabledObjects.Add(@object);
                }
            }
        }

        foreach (var @object in disabledObjects)
        {
            @object.SetActive(false);
        }

        foreach (var @object in enabledObjects)
        {
            @object.SetActive(true);
        }
    }
}

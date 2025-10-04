using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Persistent Object Activity")]
    public class PersistentObjectActivity : MonoBehaviour
    {
        [SerializeField] List<Object> objects;

        public class ObjectActivityData : DatabaseObject
        {
            public List<bool> boolValues { get; set; } = new();
        }

        ObjectActivityData data;

        void Awake()
        {
            data = this.Load<ObjectActivityData>();
            for (int i = 0; i < data.boolValues.Count && i < objects.Count; i++)
                objects[i].SetActiveOrEnable(data.boolValues[i]);
        }

        void OnDestroy()
        {
            Database.Save(data);
        }

        void Update()
        {
            var objectActivity = objects.Select(x => x.IsActiveOrEnabled(activeSelf: true)).ToList();
            if (data.boolValues.SequenceEqual(objectActivity))
                return;

            data.boolValues.Clear();
            data.boolValues.AddRange(objectActivity);
            data.AddToSaveQueue<ObjectActivityData>();
        }
    }
}

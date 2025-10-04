using System;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;

using F = System.Reflection.BindingFlags;

namespace Prototype
{
    public abstract class RuntimeInstance : MonoBehaviour, IDestroyable
    {
        static Type[] _dataTypes;

        public static Type[] dataTypes => _dataTypes ??= Utils.types.Where(
            x => x.IsSubclassOf(typeof(Data))
        ).ToArray();

        [SerializeField, ReadOnly] string resource;
        [SerializeField] bool destroy;

        public abstract class Data : DatabaseObject
        {
            public string scene { get; set; } = string.Empty;
            public string resource { get; set; } = string.Empty;
        }

        public Data data { get; private set; }

        Type type;
        Type dataType;

        MethodInfo onLoad;
        MethodInfo onSave;
        MethodInfo addToSaveQueue;
        MethodInfo removeFromDatabase;

        public event Action onStart;

        void Awake()
        {
            type = GetType();
            dataType = type.GetCustomAttribute<DataAttribute>().type;

            var staticMethods = type.GetMethods(
                F.Public | F.NonPublic | F.Instance
            );
            MethodInfo GetMethod(string name) => staticMethods.FirstOrDefault(
                x => x.Name == name &&
                x.GetParameters().FirstOrDefault().ParameterType == dataType
            );
            onLoad = GetMethod("OnLoad");
            onSave = GetMethod("OnSave");
            addToSaveQueue = Database.addToSaveQueueMethod.MakeGenericMethod(dataType);
            removeFromDatabase = Database.removeFromDatabaseMethod.MakeGenericMethod(dataType);
        }

        void Start()
        {
            var unique = GetComponent<RuntimeUnique>();
            if (unique.id == 0)
            {
                var getLastId = Database.getLastIdMethod.MakeGenericMethod(dataType);
                unique.id = (int) getLastId.Invoke(null, null) + 1;
            }

            var load = Database.loadMethod.MakeGenericMethod(dataType);
            data = load.Invoke(null, new object[] { unique, null }) as Data;
            data.scene = gameObject.scene.name;
            data.resource = resource;

            onLoad.Invoke(this, new object[] { data });
            onStart?.Invoke();
        }

        void Update()
        {
            if (destroy)
            {
                Destroy();
                return;
            }

            onSave.Invoke(this, new object[] { data });
            addToSaveQueue.Invoke(null, new object[] { data });
        }

        public void Destroy()
        {
            if (!Application.isPlaying)
                return;

#if UNITY_EDITOR
            if (isInPrefabStage)
                return;
#endif

            gameObject.Kill();
            removeFromDatabase.Invoke(this, new object[] { data });
        }

        void OnDestroy()
        {
            if (!Application.isPlaying)
                return;

            if (destroy)
                return;

            var save = Database.saveMethod.MakeGenericMethod(dataType);
            save.Invoke(null, new object[] { data });
        }

#if UNITY_EDITOR
        bool isInPrefabStage =>
            PrefabStageUtility.GetPrefabStage(gameObject) ||
            gameObject.scene.name is null;

        void OnDrawGizmosSelected()
        {
            if (!isInPrefabStage)
                return;

            var name = gameObject.name;
            if (resource == name)
                return;

            resource = name;
            EditorUtility.SetDirty(gameObject);
        }
#endif
    }
}

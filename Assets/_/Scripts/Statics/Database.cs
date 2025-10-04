using LiteDB;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

using F = System.Reflection.BindingFlags;

namespace Prototype
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class Database
    {
        public static int pendingCount => pendingObjects.Count;

        static LiteDatabase database;
        static readonly object locker = new();
        static readonly HashSet<DatabaseObject> pendingObjects = new();
        static readonly Queue<Action> pendingActions = new();

#if UNITY_EDITOR
        static bool playMode;

        static Database()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    playMode = true;
                    return;
                }

                playMode = false;
                pendingObjects.Clear();
                pendingActions.Clear();
            };
        }
#endif

        public static MethodInfo GetStaticMethod(string name) => typeof(Database).GetMethod(name, F.Public | F.Static);
        public static readonly MethodInfo getLastIdMethod = GetStaticMethod(nameof(GetLastId));
        public static readonly MethodInfo findAllMethod = GetStaticMethod(nameof(FindAll));
        public static readonly MethodInfo loadMethod = GetStaticMethod(nameof(Load));
        public static readonly MethodInfo saveMethod = GetStaticMethod(nameof(Save));
        public static readonly MethodInfo addToSaveQueueMethod = GetStaticMethod(nameof(AddToSaveQueue));
        public static readonly MethodInfo removeFromDatabaseMethod = GetStaticMethod(nameof(RemoveFromDatabase));
        public static readonly MethodInfo cleanUpRuntimeInstanceDataMethod = GetStaticMethod(nameof(CleanUpRuntimeInstanceData));

        public static void LoadDatabase(string name)
        {
            Unload();
            database = LoadInternal(name);
        }

        public static void Use(string name, Action<LiteDatabase> callback)
        {
            _ = Use(name, x =>
            {
                callback(x);
                return null as object;
            });
        }

        public static T Use<T>(string name, Func<LiteDatabase, T> callback)
        {
            var database = LoadInternal(name);
            var result = callback(database);
            database.Dispose();
            return result;
        }

        public static void Delete(string name)
        {
            try { File.Delete(GetPath(name)); } catch { }
        }

        static LiteDatabase LoadInternal(string name)
        {
            return new(GetPath(name)) { Mapper = { IncludeFields = true } };
        }

        static string GetPath(string name)
        {
            return Path.Combine(Application.persistentDataPath, $"{name}.db");
        }

        public static void Unload()
        {
            if (database is null)
                return;

            foreach (var type in RuntimeInstance.dataTypes)
                cleanUpRuntimeInstanceDataMethod
                    .MakeGenericMethod(type)
                    .Invoke(null, null);

            database.Dispose();
            database = null;
        }

        public static int GetLastId<T>() where T : DatabaseObject
        {
            var collection = database.GetCollection<T>();
            if (collection.Count() == 0)
                return 0;

            return collection.Max(x => x.uniqueId);
        }

        public static IEnumerable<T> FindAll<T>() where T : DatabaseObject
        {
            if (database is null)
                yield break;

            var collection = database.GetCollection<T>();
            if (collection.Count() == 0)
                yield break;

            foreach (var item in collection.FindAll())
                yield return item;
        }

        public static T Load<T>(IUnique unique, Action<T> onCreate = default) where T : DatabaseObject, new()
        {
            T data;

            if (database is null || unique is null)
            {
                data = new T();
                onCreate?.Invoke(data);
                return data;
            }

            var collection = database.GetCollection<T>();
            var existingData = collection.FindById(unique.GetId());

            if (existingData is not null)
                return existingData;

            data = new() { uniqueId = unique.GetId() };

            onCreate?.Invoke(data);

            AddToDatabase(collection, data);

            return data;
        }

        public static void Save<T>(T data) where T : DatabaseObject
        {
            if (database is null)
                return;

            lock (locker)
            {
                if (pendingObjects.Contains(data))
                    pendingObjects.Remove(data);

                database.GetCollection<T>().Update(data);
            }
        }

        public static void AddToDatabase<T>(in T data) where T : DatabaseObject
        {
            AddToDatabase(database.GetCollection<T>(), data);
        }

        static void AddToDatabase<T>(in ILiteCollection<T> collection, in T data) where T : DatabaseObject
        {
            if (database.BeginTrans())
            {
                var id = collection.Insert(data);
                if (id == data.uniqueId)
                {
                    database.Commit();
                }
                else
                {
                    database.Rollback();
                    throw new($"{nameof(database)}.{nameof(database.Rollback)}()");
                }
            }
        }

        public static void RemoveFromDatabase<T>(in T data) where T : DatabaseObject
        {
            database.GetCollection<T>().Delete(data.uniqueId);
        }

        public static void CleanUpRuntimeInstanceData<T>() where T : RuntimeInstance.Data
        {
            database.GetCollection<T>().DeleteMany(
                x => string.IsNullOrEmpty(x.scene) ||
                     string.IsNullOrEmpty(x.resource)
            );
        }

        public static void AddToSaveQueue<T>(T data) where T : DatabaseObject
        {
            if (data.uniqueId == 0)
                return;

            if (pendingObjects.Contains(data))
                return;

            lock (locker)
            {
                pendingObjects.Add(data);
                pendingActions.Enqueue(() =>
                {
                    lock (locker)
                    {
                        pendingObjects.Remove(data);
                        database.GetCollection<T>().Update(data);
                    }
                });
            }
        }

        public static bool SaveNext()
        {
#if UNITY_EDITOR
            if (!playMode)
                return false;
#endif

            if (pendingActions.Count == 0)
                return false;

            lock (locker)
            {
                pendingActions.Dequeue()();
                return true;
            }
        }

        public static void SaveAll()
        {
            lock (locker)
                while (SaveNext()) ;
        }
    }
}

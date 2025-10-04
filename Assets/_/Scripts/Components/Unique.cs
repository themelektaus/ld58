using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Unique")]
    public class Unique : MonoBehaviour, IUnique
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void ClearUniqueInfoCache() => _uniqueInfoCache = null;

        static HashSet<UniqueInfo> _uniqueInfoCache;

        public static HashSet<UniqueInfo> uniqueInfoCache
            => _uniqueInfoCache ??= FindAllUniqueInfos();
#endif

        [ReadOnly] public int id;
        [ReadOnly] public string gameObjectName;

        public int GetId() => id;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            this.DrawEditorLabel("ID", id.ToString(), new() { offset = new(-.425f, 0) });
        }

        bool isInPrefabStage =>
            PrefabStageUtility.GetPrefabStage(gameObject) ||
            gameObject.scene.name is null;

        void OnDrawGizmosSelected()
        {
            gameObjectName = gameObject.name;

            if (id > 0)
                return;

            Apply();
        }

        void Apply()
        {
            if (isInPrefabStage)
            {
                var gameObject = this.gameObject;
                DestroyImmediate(this);
                EditorUtility.SetDirty(gameObject);
                return;
            }

            if (this.id != 0)
                return;

            ClearUniqueInfoCache();
            var ids = uniqueInfoCache.Select(x => x.id).ToHashSet();

            ids.UnionWith(
                FindObjectsByType<Unique>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(x => x != this)
                .Where(x => x.id != 0)
                .Select(x => x.id));

            var id = this.id;

            do id++;
            while (ids.Contains(id));

            if (this.id == id)
                return;

            this.id = id;
            EditorUtility.SetDirty(this);
        }

        public struct UniqueInfo
        {
            public string scene;
            public string name;
            public int id;

            public override string ToString()
            {
                return $"{id}{(name.IsNullOrEmpty() ? "" : $": {name}")} ({scene})";
            }
        }

        static HashSet<UniqueInfo> FindAllUniqueInfos([CallerFilePath] string path = null)
        {
            var infos = new HashSet<UniqueInfo>();

            var scriptPath = Path.GetRelativePath(".", path);
            var scriptGuid = AssetDatabase.GUIDFromAssetPath(scriptPath).ToString();

            List<string> lines = null;

            void Check(string assetName)
            {
                if (lines is null)
                    return;

                if (lines.Any(x => x.StartsWith("  m_Script:") && x.Contains("guid:") && x.Contains(scriptGuid)))
                {
                    var idLine = lines.FirstOrDefault(x => x.StartsWith("  id:"));
                    var id = idLine[5..].Trim();

                    var gameObjectNameLine = lines.FirstOrDefault(x => x.StartsWith("  gameObjectName:"));
                    var gameObjectName = gameObjectNameLine?[17..].Trim() ?? string.Empty;

                    infos.Add(new()
                    {
                        scene = assetName,
                        name = gameObjectName,
                        id = int.Parse(id)
                    });
                }
                lines = null;
            }

            var assetGuids = AssetDatabase.FindAssets($"t:{nameof(SceneAsset)}");
            foreach (var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetName = Path.GetFileNameWithoutExtension(assetPath);
                var assetLines = File.ReadAllLines(assetPath);

                foreach (var line in assetLines)
                {
                    if (lines is not null)
                    {
                        if (line.StartsWith("---"))
                            Check(assetName);
                        else
                            lines.Add(line);
                    }

                    if (line.StartsWith("--- !u!114"))
                        lines = new();
                }

                Check(assetName);
            }

            return infos;
        }
#endif
    }
}

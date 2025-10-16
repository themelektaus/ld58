using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using F = System.Reflection.BindingFlags;

namespace Prototype.Editor
{
    [ExecuteAlways]
    public class PlaySceneWindow : EditorWindow
    {
        const string TITLE = "Play Scene";

        [MenuItem(Const.MENU_ASSETS + "/" + TITLE)]
        public static void Open()
            => _ = GetWindow<PlaySceneWindow>(TITLE);

        [SerializeField] bool enabled = true;

        [SerializeField] string lastActiveScene;
        [SerializeField] List<string> lastScenePaths = new();

        [SerializeField] string scenePath;
        [SerializeField] int sceneIndex;

        static List<(string path, SceneCollection collection)> sceneCollections;
        static string[] scenePaths;
        static string[] sceneNames;

        static string activeScenePath
            => SceneManager.GetActiveScene().path;

        void OnEnable()
        {
            OnDisable();

            RefreshSceneCollections();

            scenePaths = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
            sceneNames = scenePaths.Select(Path.GetFileNameWithoutExtension).ToArray();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void RefreshSceneCollections()
        {
            sceneCollections = AssetDatabase.FindAssets($"t:{nameof(SceneCollection)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SceneCollection>)
                .Where(x => x)
                .Select(x => (
                    string.Join(";", x.sceneAssets.Select(AssetDatabase.GetAssetPath)),
                    x
                ))
                .ToList();
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!enabled)
                return;

            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorApplication.ExitPlaymode();

            lastActiveScene = SceneManager.GetActiveScene().path;
            lastScenePaths = Enumerable
                .Range(0, SceneManager.sceneCount)
                .Select(i => SceneManager.GetSceneAt(i).path)
                .ToList();

            scenePath = scenePaths[sceneIndex];
            EditorSceneManager.OpenScene(scenePath);
        }

        void Update()
        {
            if (!enabled)
                return;

            if (EditorApplication.isPlaying)
                return;

            if (lastScenePaths.Count == 0)
                return;

            if (scenePath is not null)
                return;

            var scene = lastScenePaths.First();
            if (scene == string.Empty)
                return;

            EditorSceneManager.OpenScene(scene);
            foreach (var scenePath in lastScenePaths.Skip(1))
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            if (!lastActiveScene.IsNullOrEmpty())
            {
                var activeScene = SceneManager.GetSceneByPath(lastActiveScene);
                if (activeScene.IsValid())
                    SceneManager.SetActiveScene(activeScene);
                lastActiveScene = string.Empty;
            }

            lastScenePaths.Clear();
        }

        static GUIStyle blockStyle1;
        static GUIStyle blockStyle2;
        static GUIStyle buttonStyle;

        void OnGUI()
        {
            blockStyle1 ??= new() { padding = new(6, 6, 6, 3) };
            blockStyle2 ??= new() { padding = new(6, 6, 3, 6) };
            buttonStyle ??= new(GUI.skin.button) { padding = new(3, 3, 1, 1) };

            System.Action action = null;

            var isPlaying = EditorApplication.isPlaying;

            GUI.FocusControl(null);

            GUILayout.BeginHorizontal(blockStyle1);
            {
                GUI.enabled = !isPlaying;

                var path = string.Join(";", Enumerable
                    .Range(0, SceneManager.sceneCount)
                    .Select(i => SceneManager.GetSceneAt(i).path)
                );

                var options = sceneCollections.Select(x => x.collection.name);

                var index = sceneCollections.FindIndex(x => x.path == path);
                if (index == -1)
                {
                    index = 0;
                    options = options.Prepend("(New Scene Collection)");
                    var newIndex = EditorGUILayout.Popup(index, options.ToArray());
                    if (newIndex != index)
                        sceneCollections[newIndex - 1].collection.Open();
                    if (GUILayout.Button("Create", buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        var sceneCollection = CreateInstance<SceneCollection>();
                        sceneCollection.sceneAssets.AddRange(
                            Enumerable
                            .Range(0, SceneManager.sceneCount)
                            .Select(i => SceneManager.GetSceneAt(i).path)
                            .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>)
                        );
                        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Untitled Scene Collection.asset");
                        AssetDatabase.CreateAsset(sceneCollection, assetPath);
                        AssetDatabase.Refresh();
                        RefreshSceneCollections();
                    }
                }
                else
                {
                    var newIndex = EditorGUILayout.Popup(index, options.ToArray());
                    if (newIndex != index)
                        sceneCollections[newIndex].collection.Open();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    action = OnEnable;
                }
            }
            GUILayout.EndHorizontal();

            if (enabled)
            {
                if (isPlaying)
                {
                    GUI.color = new(.5f, 1, .5f);
                    GUI.enabled = false;
                    if (scenePath is not null && scenePath == activeScenePath)
                        scenePath = null;
                }
                else
                {
                    if (enabled)
                        GUI.color = new(.75f, 1, .75f);

                    if (scenePath == activeScenePath)
                        EditorApplication.EnterPlaymode();
                }
            }

            GUILayout.BeginHorizontal(blockStyle2);
            {
                enabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(16));
                GUI.enabled = enabled && !isPlaying;
                sceneIndex = EditorGUILayout.Popup(sceneIndex, sceneNames);

                GUI.enabled = !isPlaying;
                var disabled = !EditorSettings.enterPlayModeOptionsEnabled;
                GUI.color = disabled ? new(1, .75f, .5f) : new(.825f, .825f, .825f);
                disabled = EditorGUILayout.Toggle(disabled, GUILayout.Width(14));
                EditorGUILayout.LabelField("Reload Domain", GUILayout.Width(86));
                EditorSettings.enterPlayModeOptionsEnabled = !disabled;
                EditorSettings.enterPlayModeOptions = disabled
                    ? EnterPlayModeOptions.None
                    : (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload);
            }
            GUILayout.EndHorizontal();

            action?.Invoke();
        }

        [MenuItem(Const.MENU_ASSETS + "/" + TITLE + " (Focus Selection)")]
        static void FocusSelection()
        {
            Collapse();

            var gameObject = Selection.activeGameObject;
            if (gameObject)
            {
                EditorGUIUtility.PingObject(gameObject);
                SceneManager.SetActiveScene(Selection.activeGameObject.scene);
            }

            var rootGameObjects = SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            var parent = rootGameObjects.FirstOrDefault(
                x => x.TryGetComponent(out DefaultParentForEditor _)
            );

            if (parent)
            {
                EditorUtility.SetDefaultParentObject(parent);
                if (!gameObject)
                    gameObject = parent;
            }

            if (!gameObject)
            {
                gameObject = rootGameObjects.FirstOrDefault();
                if (!gameObject)
                    return;
            }

            if (parent)
            {
                var transform = gameObject.transform;
                if (transform.childCount > 0)
                    gameObject = transform.GetChild(0).gameObject;
            }

            EditorGUIUtility.PingObject(gameObject);
        }

        [MenuItem(Const.MENU_ASSETS + "/" + TITLE + " (Focus Scene)")]
        static void FocusScene()
        {
            Collapse();

            var gameObject = Selection.activeGameObject;
            if (gameObject)
                SceneManager.SetActiveScene(Selection.activeGameObject.scene);

            var rootGameObjects = SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            gameObject = rootGameObjects.FirstOrDefault(
                x => x.TryGetComponent(out DefaultParentForEditor _)
            );

            if (gameObject)
            {
                EditorUtility.SetDefaultParentObject(gameObject);
                var transform = gameObject.transform;
                if (transform.childCount > 0)
                    gameObject = transform.GetChild(0).gameObject;
            }
            else
            {
                gameObject = rootGameObjects.FirstOrDefault();
                if (!gameObject)
                    return;
            }

            EditorGUIUtility.PingObject(gameObject);
            Selection.activeObject = null;
        }

        static void Collapse()
        {
            var activeObject = Selection.activeObject;
            Collapse(repaint: false);
            Collapse(repaint: true);
            Selection.activeObject = activeObject;
        }

        static void Collapse(bool repaint)
        {
            Selection.activeObject = null;

            var scopeFlags = F.Public | F.NonPublic;
            var instanceFlags = scopeFlags | F.Instance;

            var window = typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow")
                .GetField("s_LastInteractedHierarchy", scopeFlags | F.Static)
                .GetValue(null) as EditorWindow;

            if (!window)
                return;

            var treeOwner = window
                .GetType()
                .GetField("m_SceneHierarchy", instanceFlags)
                .GetValue(window);

            var tree = treeOwner
                .GetType()
                .GetField("m_TreeView", instanceFlags)
                .GetValue(treeOwner);

            if (tree is null)
                return;

            var source = tree
                .GetType()
                .GetProperty("data", instanceFlags)
                .GetValue(tree, null);

            source
                .GetType()
                .GetMethod("SetExpandedIDs", instanceFlags)
                .Invoke(source, new[] { new int[0] });

            if (repaint)
                window.Repaint();
        }
    }
}

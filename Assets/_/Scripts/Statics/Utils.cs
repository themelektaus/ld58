using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

using AppDomain = System.AppDomain;

#if UNITY_EDITOR
using static UnityEditor.AssetDatabase;
#endif

namespace Prototype
{
    public static class Utils
    {
        #region Assemblies
        static readonly string[] assemblyInclusions = new[] {
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.Physics2DModule",
            "UnityEngine.TilemapModule",
            "UnityEditor",
            "UnityEditor.CoreModule"
        };

        static readonly string[] assemblyExclusions = new[] {
            "0Harmony",
            "Bee",
            "Cinemachine",
            "clipper_library",
            "com.unity.cinemachine.editor",
            "ExCSS",
            "HarmonySharedState",
            "HBAO",
            "Mono",
            "Microsoft.CSharp",
            "mscorlib",
            "netstandard",
            "nunit.framework",
            "PlayerBuildProgramLibrary",
            "PsdPlugin",
            "ScriptCompilationBuildProgram",
            "System",
            "Unity",
            "WebGLPlayerBuildProgram"
        };
        #endregion

        static HashSet<Assembly> _assemblies;
        static HashSet<Assembly> assemblies
        {
            get
            {
                _assemblies ??= new();

                if (_assemblies.Count == 0)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var name = assembly.FullName;

                        if (assemblyInclusions.Any(x => name.Split(',')[0] == x))
                        {
                            _assemblies.Add(assembly);
                            continue;
                        }

                        if (!assemblyExclusions.Any(x => name.StartsWith(x)))
                        {
                            _assemblies.Add(assembly);
                            continue;
                        }
                    }
                }

#if UNITY_EDITOR
                System.IO.Directory.CreateDirectory("Temp");
                System.IO.File.WriteAllLines(
                    @"Temp\Assemblies.txt",
                    _assemblies.Select(x => x.FullName.Split(',')[0]).ToList()
                );
#endif
                return _assemblies;
            }
        }

        static HashSet<System.Type> _types;
        public static HashSet<System.Type> types
        {
            get
            {
                if (_types is null || _types.Count == 0)
                    _types = assemblies.SelectMany(x => x.GetTypes()).ToHashSet();

                return _types;
            }
        }

        public static void DestroyImmediateInEditor(Object obj)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
                return;
            }

            Object.DestroyImmediate(obj);
        }

        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        public static bool Approximately(float a, float b, float threshold)
        {
            return Mathf.Abs(a - b) < threshold;
        }

        public static bool Approximately(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x)
                && Mathf.Approximately(a.y, b.y);
        }

        public static bool Approximately(Vector2 a, Vector2 b, float threshold)
        {
            return Approximately(a.x, b.x, threshold)
                && Approximately(a.y, b.y, threshold);
        }

        public static bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x)
                && Mathf.Approximately(a.y, b.y)
                && Mathf.Approximately(a.z, b.z);
        }

        public static bool Approximately(Vector3 a, Vector3 b, float threshold)
        {
            return Approximately(a.x, b.x, threshold)
                && Approximately(a.y, b.y, threshold)
                && Approximately(a.z, b.z, threshold);
        }

        public static bool Approximately(Quaternion a, Quaternion b)
        {
            return Approximately(a.x, b.x)
                && Approximately(a.y, b.y)
                && Approximately(a.z, b.z)
                && Approximately(a.w, b.w);
        }

        public static bool Approximately(Quaternion a, Quaternion b, float threshold)
        {
            return Approximately(a.x, b.x, threshold)
                && Approximately(a.y, b.y, threshold)
                && Approximately(a.z, b.z, threshold)
                && Approximately(a.w, b.w, threshold);
        }

        public static float Clamp(float value, Vector2 range)
        {
            return Mathf.Clamp(value, range.x, range.y);
        }

        public static T RandomPick<T>(IList<T> collection)
        {
            int index = Random.Range(0, collection.Count);
            return collection[index];
        }

        public static T RandomPick<T>() where T : System.Enum
        {
            var values = typeof(T).GetEnumValues();
            int index = Random.Range(0, values.Length);
            return (T) values.GetValue(index);
        }

        public static float RandomFloat()
        {
            float value;
            do { value = Random.value; }
            while (Approximately(value, 1));
            return value;
        }

        public static bool RandomTrue(float probability = .5f)
        {
            return RandomFloat() < probability;
        }

        public static int RandomRange(Vector2Int range)
        {
            return RandomRange(range.x, range.y);
        }

        public static int RandomRange(int min, int max)
        {
            return Random.Range(min, max + 1);
        }

        public static float RandomRange(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }

        public static Vector2 RandomRange(Vector2 min, Vector2 max)
        {
            return new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );
        }

        public static Vector3 RandomRange(Vector3 min, Vector3 max)
        {
            return new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );
        }

        public static Vector3 RandomPointInsideCollider(Collider collider)
        {
            var bounds = collider.bounds;
            var point = RandomPointInsideBounds(bounds);

            if (point.IsInside(collider))
                return point;

            return RandomPointInsideCollider(collider);
        }

        public static Vector3 RandomPointInsideBounds(Bounds bounds)
        {
            return RandomRange(bounds.min, bounds.max);
        }

        public static Vector2 RandomPointInsideCollider(Collider2D collider)
        {
            var bounds = collider.bounds;
            var point = RandomRange((Vector2) bounds.min, (Vector2) collider.bounds.max);

            var closestPoint = collider.ClosestPoint(point);
            if (!Approximately(point, closestPoint))
                return RandomPointInsideCollider(collider);

            return point;
        }

        public static Vector2 Abs(Vector2 value)
        {
            return new(Mathf.Abs(value.x), Mathf.Abs(value.y));
        }

        public static Vector2 Round(Vector2 value)
        {
            return new(Mathf.Round(value.x), Mathf.Round(value.y));
        }

        public static Vector3 Round(Vector3 value)
        {
            return new(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z));
        }

        public static T Invoke<T>(System.Func<T> func, T @default = default)
        {
            if (func is null)
                return @default;
            return func.Invoke();
        }

#if UNITY_EDITOR
        public static Texture2D Screenshot(Rect rect)
        {
            var position = new Vector2Int((int) rect.x, (int) rect.y);
            var size = new Vector2Int((int) rect.width, (int) rect.height);
            var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, size.x, size.y);
            var texture = new Texture2D(size.x, size.y);
            texture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
#endif



        static readonly FrameTimeVariable<List<RaycastResult>> pointerRaycastResults = new(x =>
        {
            x.Clear();
            var e = EventSystem.current;
            if (e) e.RaycastAll(new(e) { position = Input.mousePosition }, x);
            return x;
        });

        public static List<RaycastResult> GetPointerRaycastResults()
            => pointerRaycastResults;

        public static bool IsMouseButton_ButNotUI(int button)
        {
            if (!Input.GetMouseButton(button))
                return false;

            if (IsPointerOverUI())
                return false;

            return true;
        }

        static readonly int uiLayer = LayerMask.NameToLayer("UI");

        public static bool IsPointerOverUI()
        {
            var results = GetPointerRaycastResults();
            return results.Any(x => x.gameObject.layer == uiLayer);
        }

        public static bool IsPointerOver(GameObject gameObject)
        {
            foreach (var result in GetPointerRaycastResults())
                if (result.gameObject == gameObject)
                    return true;
            return false;
        }

        public static bool IsMouseButtonDown_ButNotUI(int button)
        {
            if (!Input.GetMouseButtonDown(button))
                return false;

            if (IsPointerOverUI())
                return false;

            return true;
        }

        public static bool IsMouseButtonUp_ButNotUI(int button)
        {
            if (!Input.GetMouseButtonUp(button))
                return false;

            if (IsPointerOverUI())
                return false;

            return true;
        }



        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;

            public static explicit operator Vector2Int(POINT point)
            {
                return new(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public static Vector2Int GetMousePosition()
        {
            GetCursorPos(out var point);
            return (Vector2Int) point;
        }

        public static void SetMousePosition(Vector2Int position)
        {
            SetCursorPos(position.x, position.y);
        }

        public static Camera GetMainCamera(bool autoCreate)
        {
            var camera = Camera.main;
            if (!camera && autoCreate)
            {
                GameObject cameraObject = new("Main Camera") { tag = "MainCamera" };
                camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new(.135f, .135f, .142f);
                cameraObject.AddComponent<AudioListener>();
            }
            return camera;
        }

        public static int CompareHierarchy(Transform a, Transform b)
        {
            if (a == b)
                return 0;

            if (b.IsChildOf(a))
                return -1;

            if (a.IsChildOf(b))
                return 1;

            static List<Transform> GetParents(Transform t)
            {
                var tt = new List<Transform> { t };
                while (t.parent)
                    tt.Add(t = t.parent);
                return tt;
            }

            var aParents = GetParents(a);
            var bParents = GetParents(b);

            for (var i = 0; i < aParents.Count; i++)
            {
                if (b.IsChildOf(aParents[i]))
                {
                    var j = bParents.IndexOf(aParents[i--]) - 1;
                    return aParents[i].GetSiblingIndex() - bParents[j].GetSiblingIndex();
                }
            }

            return aParents[^1].GetSiblingIndex() - bParents[^1].GetSiblingIndex();
        }

        static readonly Dictionary<string, Object> resourcesCache = new();

        public static T GetResource<T>(string path) where T : Object
        {
            if (path.IsNullOrEmpty())
                return default;

            if (!resourcesCache.ContainsKey(path))
                resourcesCache.Add(path, Resources.Load<T>(path));

            return resourcesCache[path] as T;
        }

        public static string FormatString(this int @this, char thousandsSeparator = ' ')
        {
            var s = @this.ToString();

            var stack = new Stack<string>();
            while (s.Length > 3)
            {
                stack.Push(s.Substring(s.Length - 3, 3));
                s = s[..^3];
            }
            if (s.Length > 0)
                stack.Push(s);

            return string.Join(thousandsSeparator, stack.ToList());
        }

        //public enum ActivityScope { Any, Self, Hierarchy }
        //public static GameObject GetSelected(ActivityScope activityScope = ActivityScope.Any)
        public static GameObject GetSelected()
        {
            var e = EventSystem.current;

            var current = e.currentSelectedGameObject;

            if (!current)
                return null;

            //if (activityScope == ActivityScope.Self && !current.activeSelf) return null;
            //if (activityScope == ActivityScope.Hierarchy && !current.activeInHierarchy) return null;

            return current;
        }

        public static bool Try<T>(T @object, System.Action<T> callback) where T : Object
        {
            if (!@object)
                return false;

            callback(@object);
            return true;
        }

        public static TResult Try<T, TResult>(T @object, System.Func<T, TResult> callback) where T : Object
        {
            if (!@object)
                return default;

            return callback(@object);
        }

#if UNITY_EDITOR
        static readonly Dictionary<(System.Type type, string name), Object> assets = new();

        public static T LoadAsset<T>(string name) where T : Object
        {
            var type = typeof(T);
            var key = (type, name);
            if (!assets.ContainsKey(key))
            {
                var filter = $"t:{type.Name} {name}";
                var assetGUID = FindAssets(filter)[0];
                var assetPath = GUIDToAssetPath(assetGUID);
                var asset = LoadAssetAtPath<T>(assetPath);
                assets.Add(key, asset);
            }
            return assets[key] as T;
        }
#endif
    }
}

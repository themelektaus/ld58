using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Prototype
{
    public static class ExtensionMethods
    {
        public static T AddComponent<T>(this GameObject @this, Action<T> onAdd) where T : Component
        {
            bool activeSelf = @this.activeSelf;
            if (activeSelf)
                @this.SetActive(false);
            var component = @this.AddComponent<T>();
            onAdd(component);
            if (activeSelf)
                @this.SetActive(true);
            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject @this) where T : Component
        {
            var component = @this.GetComponent<T>();
            if (!component)
                component = @this.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject @this, Action<T> onAdd) where T : Component
        {
            var component = @this.GetComponent<T>();
            if (!component)
                component = @this.AddComponent(onAdd);
            return component;
        }

        public static void RemoveComponent<T>(this GameObject @this) where T : Component
        {
            var component = @this.GetComponent<T>();
            Object.Destroy(component);
        }

        public static GameObject Instantiate(this GameObject @this)
        {
            return @this.InstantiateInternal(new());
        }

        public static GameObject Instantiate(this GameObject @this, Action<GameObject> onInstantiate)
        {
            return @this.InstantiateInternal(new() { onInstantiate = onInstantiate });
        }

        public static GameObject Instantiate(this GameObject @this, Transform parent)
        {
            return @this.InstantiateInternal(new() { parent = parent });
        }

        public static GameObject Instantiate(this GameObject @this, Transform parent, Action<GameObject> onInstantiate)
        {
            return @this.InstantiateInternal(new() { parent = parent, onInstantiate = onInstantiate });
        }

        public static GameObject Instantiate(this GameObject @this, Vector3 position)
        {
            return @this.InstantiateInternal(new() { position = position });
        }

        public static GameObject Instantiate(this GameObject @this, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return @this.InstantiateInternal(new() { position = position, rotation = rotation, scale = scale });
        }

        struct InstantiationOptions
        {
            public Transform parent;
            public Vector3? position;
            public Quaternion? rotation;
            public Vector3? scale;
            public Action<GameObject> onInstantiate;
        }

        static GameObject InstantiateInternal(this GameObject @this, InstantiationOptions options)
        {
            GameObject gameObject;

                gameObject = Object.Instantiate(@this, options.parent);

            bool activeSelf = @this.activeSelf;

            if (options.onInstantiate is not null)
            {
                if (activeSelf)
                    gameObject.SetActive(false);

                options.onInstantiate(gameObject);
            }

            var transform = gameObject.transform;

            if (options.position.HasValue)
                transform.position = options.position.Value;

            if (options.rotation.HasValue)
                transform.rotation = options.rotation.Value;

            if (options.scale.HasValue)
                transform.localScale = options.scale.Value;

            if (options.onInstantiate is not null && activeSelf)
                gameObject.SetActive(true);

            return gameObject;
        }

        public static T DontDestroy<T>(this T @this) where T : Object
        {
            Object.DontDestroyOnLoad(@this);
            return @this;
        }

        public static void Destroy(this Object @this)
        {
            if (@this is not GameObject gameObject)
            {
                Object.Destroy(@this);
                return;
            }

            if (gameObject.TryGetComponent<IDestroyable>(out var destroyable))
            {
                destroyable.Destroy();
                return;
            }

            gameObject.Kill();
        }

        public static List<Transform> GetChildren(this Transform @this)
        {
            var transforms = new List<Transform>();

            foreach (Transform transform in @this)
                transforms.Add(transform);

            return transforms;
        }

        public static List<GameObject> GetChildren(this GameObject @this)
        {
            return @this.transform.GetChildren().Select(x => x.gameObject).ToList();
        }

        public static bool Contains(this Transform @this, Transform other)
        {
            foreach (Transform child in @this)
                if (child == other || child.Contains(other))
                    return true;
            return false;
        }

        public static void DestroyChildren(this Transform @this)
        {
            @this.gameObject.DestroyChildren();
        }

        public static void DestroyChildren(this Transform @this, Predicate<GameObject> condition)
        {
            @this.gameObject.DestroyChildren(condition);
        }

        public static void DestroyChildren(this GameObject @this)
        {
            @this.GetChildren().ForEach(Destroy);
        }

        public static void DestroyChildren(this GameObject @this, Predicate<GameObject> condition)
        {
            foreach (var child in @this.GetChildren().Where(x => condition(x)))
                child.Destroy();
        }

        public static void DestroyChildrenImmediate(this Transform @this)
        {
            @this.gameObject.DestroyChildrenImmediate();
        }

        public static void DestroyChildrenImmediate(this GameObject @this)
        {
            @this.GetChildren().ForEach(Object.DestroyImmediate);
        }

        public static void Kill(this GameObject @this)
        {
            Object.Destroy(@this);
        }

        public static void KillChildren(this GameObject @this)
        {
            foreach (var gameObject in @this.GetChildren())
                gameObject.Kill();
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<object> @this) where T : class
        {
            return @this
                .Select(x => x as T)
                .Where(x => x is Object
                    ? (x as Object)
                    : x is not null
                );
        }

        public static string ToFormattedString(this float @this, FloatFormat format)
        {
            var @string = format switch
            {
                FloatFormat.Raw => @this.ToString(),
                FloatFormat.OneDecimal => $"{@this:0.0}",
                FloatFormat.TwoDecimals => $"{@this:0.00}",
                FloatFormat.ThreeDecimals => $"{@this:0.000}",
                FloatFormat.Percent => $"{Mathf.RoundToInt(@this * 100)}%",
                _ => "",
            };

            return @string.Replace(',', '.');
        }

        public static AnimatorControllerParameter GetParameter(this Animator @this, string name)
        {
            var parameters = @this.GetParameters();

            if (parameters.ContainsKey(name))
                return parameters[name];

            return null;
        }

        static readonly Dictionary<Animator, Dictionary<string, AnimatorControllerParameter>> animatorParameters = new();

        static Dictionary<string, AnimatorControllerParameter> GetParameters(this Animator @this)
        {
            if (animatorParameters.ContainsKey(@this))
                return animatorParameters[@this];

            var parameters = new Dictionary<string, AnimatorControllerParameter>();

            foreach (var parameter in @this.parameters)
                parameters[parameter.name] = parameter;

            animatorParameters.Add(@this, parameters);

            return parameters;
        }

        public static string[] GetParameterNames(this Animator @this)
        {
            return @this.GetParameters().Select(x => x.Key).ToArray();
        }

        public static object GetParameterValue(this Animator @this, string name)
        {
            if (!Application.isPlaying)
                return null;

            var parameter = @this.GetParameter(name);
            if (parameter is null)
                return null;

            return parameter.type switch
            {
                AnimatorControllerParameterType.Float => @this.GetFloat(name),
                AnimatorControllerParameterType.Int => @this.GetInteger(name),
                AnimatorControllerParameterType.Bool => @this.GetBool(name),
                _ => null,
            };
        }

        public static bool SetParameterValue(this Animator @this, string name, object value)
        {
            var parameter = @this.GetParameter(name);
            if (parameter is null)
                return false;

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    if (value is float floatValue)
                    {
                        @this.SetFloat(name, floatValue);
                        return true;
                    }
                    break;

                case AnimatorControllerParameterType.Int:
                    if (value is int intValue)
                    {
                        @this.SetInteger(name, intValue);
                        return true;
                    }
                    break;

                case AnimatorControllerParameterType.Bool:
                    if (value is bool boolValue)
                    {
                        @this.SetBool(name, boolValue);
                        return true;
                    }
                    break;
            }

            return false;
        }

        struct ShaderPropertyField
        {
            public bool x;
            public bool y;
            public bool z;
            public bool w;

            public bool r;
            public bool g;
            public bool b;
            public bool a;

            public static ShaderPropertyField From(string fields)
            {
                return new ShaderPropertyField
                {
                    x = fields.Contains('x'),
                    y = fields.Contains('y'),
                    z = fields.Contains('z'),
                    w = fields.Contains('w'),

                    r = fields.Contains('r'),
                    g = fields.Contains('g'),
                    b = fields.Contains('b'),
                    a = fields.Contains('a'),
                };
            }

            public bool IsVector() => x || y || z || w;
            public bool IsColor() => r || g || b || a;
        }

        public static float GetValue(this Material @this, string name)
        {
            if (!Application.isPlaying)
                return 0;

            if (name.Contains('.'))
            {
                var parts = name.Split('.');

                name = parts[0];

                var field = ShaderPropertyField.From(parts[1]);

                if (field.IsVector())
                {
                    var vector = @this.GetVector(name);

                    if (field.x) return vector.x;
                    if (field.y) return vector.y;
                    if (field.z) return vector.z;
                    if (field.w) return vector.w;
                }

                if (field.IsColor())
                {
                    var color = @this.GetColor(name);

                    if (field.r) return color.r;
                    if (field.g) return color.g;
                    if (field.b) return color.b;
                    if (field.a) return color.a;
                }
            }

            return @this.GetFloat(name);
        }

        public static void SetValue(this Material @this, string name, float value)
        {
            if (name.Contains('.'))
            {
                var parts = name.Split('.');

                name = parts[0];

                var field = ShaderPropertyField.From(parts[1]);

                if (field.IsVector())
                {
                    var vector = @this.GetVector(name);

                    if (field.x)
                        vector.x = value;

                    else if (field.y)
                        vector.y = value;

                    else if (field.z)
                        vector.z = value;

                    else if (field.w)
                        vector.w = value;

                    @this.SetVector(name, vector);

                    return;
                }

                if (field.IsColor())
                {
                    var color = @this.GetColor(name);

                    if (field.r)
                        color.r = value;

                    else if (field.g)
                        color.g = value;

                    else if (field.b)
                        color.b = value;

                    else if (field.a)
                        color.a = value;

                    @this.SetColor(name, color);

                    return;
                }
            }

            @this.SetFloat(name, value);
        }

        public static GameObject GetGameObject(this Object @this)
        {
            if (!@this)
                return null;

            if (@this is ObjectQuery objectQuery)
                return objectQuery.FindGameObject();

            if (@this is GameObject gameObject)
                return gameObject;

            if (@this is Component component)
                return component.gameObject;

            return null;
        }

        public static Transform GetTransform(this Object @this)
        {
            if (!@this)
                return null;

            if (@this is ObjectQuery objectQuery)
                return objectQuery.FindTransform();

            if (@this is Transform transform)
                return transform;

            if (@this is GameObject gameObject)
                return gameObject.transform;

            if (@this is Component component)
                return component.transform;

            return null;
        }

        public static T GetComponent<T>(this Object @this) where T : Component
        {
            if (!@this)
                return default;

            if (@this is T t)
                return t;

            if (@this is ObjectQuery objectQuery)
                return objectQuery.FindComponent<T>();

            if (@this is Transform transform)
                return transform.GetComponent<T>();

            if (@this is GameObject gameObject)
                return gameObject.GetComponent<T>();

            if (@this is Component component)
                return component.GetComponent<T>();

            return default;
        }

        public static T[] GetComponents<T>(this Object @this) where T : Component
        {
            if (!@this)
                return new T[0];

            if (@this is T t)
                return new[] { t };

            if (@this is ObjectQuery objectQuery)
                return objectQuery.FindComponents<T>();

            if (@this is Transform transform)
                return transform.GetComponents<T>();

            if (@this is GameObject gameObject)
                return gameObject.GetComponents<T>();

            if (@this is Component component)
                return component.GetComponents<T>();

            return new T[0];
        }

        public static T GetComponentInParent<T>(this Object @this) where T : Component
        {
            if (!@this)
                return default;

            if (@this is T t)
                return t;

            if (@this is ObjectQuery objectQuery)
                return objectQuery.FindComponentInParent<T>();

            if (@this is Transform transform)
                return transform.GetComponentInParent<T>();

            if (@this is GameObject gameObject)
                return gameObject.GetComponentInParent<T>();

            if (@this is Component component)
                return component.GetComponentInParent<T>();

            return default;
        }

        public static bool Match(this Object @object, Object other)
        {
            if (@object is ObjectQuery query)
                return query.Match(other);

            return @object == other;
        }

        public static Vector2 ToXZ(this Vector3 @this)
        {
            return new Vector2(@this.x, @this.z);
        }

        public static Vector3 ToX0Z(this Vector2 @this)
        {
            return new Vector3(@this.x, 0, @this.y);
        }

        public static Vector3 ToXYZ(this Vector2 @this, float y)
        {
            return new Vector3(@this.x, y, @this.y);
        }

        public static void SubscribeTo<T>(this IObserver<T> @this, IObservable<T> observable) where T : IMessage
        {
            observable.Register(@this);
        }

        public static void UnsubscribeFrom<T>(this IObserver<T> @this, IObservable<T> observable) where T : IMessage
        {
            observable.Unregister(@this);
        }

        public static bool IsInside(this Vector3 @this, Collider collider)
        {
            return Utils.Approximately(@this, collider.ClosestPoint(@this));
        }

        public static bool IsInside(this Vector2 @this, Collider2D collider)
        {
            return Utils.Approximately(@this, collider.ClosestPoint(@this));
        }

        public static Collider[] CheckCollisions(this Collider @this)
        {
            var transform = @this.transform;

            if (@this is BoxCollider boxCollider)
            {
                var center = transform.TransformPoint(boxCollider.center);
                var halfExtents = Vector3.Scale(boxCollider.size / 2, transform.lossyScale);
                var orientation = transform.rotation;
                return Physics.OverlapBox(center, halfExtents, orientation);
            }

            if (@this is SphereCollider sphereCollider)
            {
                var center = transform.TransformPoint(sphereCollider.center);
                var radius = sphereCollider.radius * transform.lossyScale.x;
                return Physics.OverlapSphere(center, radius);
            }

            Debug.LogWarning($"{@this.GetType()} is not supported");

            return Array.Empty<Collider>();
        }

        public static Quaternion? GetLookRotation(this Transform @this, Vector3 target)
        {
            var dir = (target - @this.position).normalized;

            if (Utils.Approximately(dir, Vector3.zero))
                return null;

            return Quaternion.LookRotation(dir);
        }

        public static Texture2D AsBase64ToTexture(this string @this)
        {
            var texture = new Texture2D(1, 1);
            texture.LoadImage(Convert.FromBase64String(@this));
            return texture;
        }

        public static string ToBase64String(this Texture2D @this)
        {
            byte[] a = @this.EncodeToJPG(80);
            return Convert.ToBase64String(a);
        }

        public static void Scale(this Texture2D @this, int width, int height, bool keepAspectRatio)
        {
            if (@this.width == width && @this.height == height)
                return;

            var filterMode = @this.filterMode;

            if (@this.filterMode != FilterMode.Trilinear)
            {
                @this.filterMode = FilterMode.Trilinear;
                @this.Apply();
            }

            var renderTexture = new RenderTexture(width, height, 24);

            Graphics.SetRenderTarget(renderTexture);

            GL.LoadPixelMatrix(0, 1, 1, 0);
            GL.Clear(true, true, new(0, 0, 0, 0));

            var rect = new Rect(0, 0, 1, 1);

            if (keepAspectRatio)
            {
                var r1 = (float) @this.width / @this.height;
                var r2 = (float) width / height;

                if (r1 > r2)
                {
                    rect.width = r1 / r2;
                    rect.x -= (rect.width - 1) / 2;
                }
                else if (r1 < r2)
                {
                    rect.height = r2 / r1;
                    rect.y -= (rect.height - 1) / 2;
                }
            }

            Graphics.DrawTexture(rect, @this);

            @this.Reinitialize(width, height);
            @this.ReadPixels(new(0, 0, width, height), 0, 0);

            Graphics.SetRenderTarget(null);

            renderTexture.Release();
            Destroy(renderTexture);

            if (@this.filterMode != filterMode)
                @this.filterMode = filterMode;

            @this.Apply();
        }

        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> @this, Func<TKey, TValue, bool> match)
        {
            var keys = @this.Keys.ToArray();
            foreach (var key in keys.Where(key => match(key, @this[key])))
                @this.Remove(key);
        }

        public static Sequence CreateSequence(this MonoBehaviour @this)
            => new(@this);

        public static Sequence CreateSequence(this MonoBehaviour @this, Action action)
            => @this.CreateSequence().Then(action);

        public static Sequence CreateSequence(this MonoBehaviour @this, Action<Sequence.Instance> action)
            => @this.CreateSequence().Then(action);

        public static Sequence Wait(this MonoBehaviour @this, float seconds)
            => @this.CreateSequence().Wait(seconds);

        public static Sequence WaitForFrame(this MonoBehaviour @this)
            => @this.CreateSequence().WaitForFrame();

        public static Sequence WaitForFrames(this MonoBehaviour @this, int frameCount)
            => @this.CreateSequence().WaitForFrames(frameCount);

        public static Sequence Wait(this MonoBehaviour @this, Func<float> seconds)
            => @this.CreateSequence().Wait(seconds);

        public static Sequence While(this MonoBehaviour @this, Func<bool> condition)
            => @this.CreateSequence().While(condition);

        public static Sequence.Instance Start(this MonoBehaviour @this, System.Collections.IEnumerator enumerator)
            => @this.CreateSequence().Then(enumerator).Start();

        public static Sequence.Instance LateStart(this MonoBehaviour @this)
            => @this.WaitForFrame().Start();

        public static Sequence.Instance LateStart(this MonoBehaviour @this, Action action)
            => @this.WaitForFrame().Start(action);

        public static Sequence.Instance LateStart(this MonoBehaviour @this, Action<Sequence.Instance> action)
            => @this.WaitForFrame().Start(action);

        public static void Enable(this Object @this) => @this.Enable(true);
        public static void Disable(this Object @this) => @this.Enable(false);
        static void Enable(this Object @this, bool enabled)
        {
            if (@this is Behaviour behaviour)
            {
                behaviour.enabled = enabled;
                return;
            }

            if (@this is Renderer renderer)
            {
                renderer.enabled = enabled;
                return;
            }

            if (@this is Collider collider)
            {
                collider.enabled = enabled;
                return;
            }

            @this.LogWarning(enabled ? "Enable" : "Disable");
        }

        public static bool IsActiveOrEnabled(this Object @this, bool activeSelf = false)
        {
            if (@this is ObjectQuery query)
            {
                var _gameObject = query.FindGameObject();
                if (_gameObject)
                    return activeSelf ? _gameObject.activeSelf : _gameObject.activeInHierarchy;

                var _object = query.FindObject();
                if (_object)
                    return _object.IsEnabled();

                return false;
            }

            if (@this is GameObject gameObject)
                return activeSelf ? gameObject.activeSelf : gameObject.activeInHierarchy;

            gameObject = @this.GetGameObject();
            if (gameObject && activeSelf ? !gameObject.activeSelf : !gameObject.activeInHierarchy)
                return false;

            return @this.IsEnabled();
        }

        public static void SetActiveOrEnable(this Object @this, bool value)
        {
            if (@this is ObjectQuery query)
            {
                var _gameObject = query.FindGameObject();
                if (_gameObject)
                {
                    _gameObject.SetActive(value);
                    return;
                }

                var _object = query.FindObject();
                if (_object)
                {
                    _object.Enable(value);
                    return;
                }

                throw new NullReferenceException();
            }

            if (@this is GameObject gameObject)
            {
                gameObject.SetActive(value);
                return;
            }

            @this.Enable(value);
        }

        public static bool IsEnabled(this Object @this)
        {
            if (@this is Behaviour behaviour)
                return behaviour.enabled;

            if (@this is Renderer renderer)
                return renderer.enabled;

            if (@this is Collider collider)
                return collider.enabled;

            @this.LogWarning("Unsupported");
            return false;
        }

        public static int Pow(this int @this, int p)
        {
            if (p < 0)
                throw new("p less than zero is not supported");

            if (p == 0)
                return 0;

            int result = @this;

            for (int i = 1; i < p; i++)
                result *= @this;

            return result;
        }

        public static float RoundTo(this float @this, int decimals)
        {
            int power = 10.Pow(decimals);
            return Mathf.Round(@this * power) / power;
        }

        public static Vector2 RoundTo(this Vector2 @this, int decimals)
        {
            var power = 10.Pow(decimals);
            @this.x = Mathf.Round(@this.x * power) / power;
            @this.y = Mathf.Round(@this.y * power) / power;
            return @this;
        }

        public static Vector3 RoundTo(this Vector3 @this, int decimals)
        {
            var power = 10.Pow(decimals);
            @this.x = Mathf.Round(@this.x * power) / power;
            @this.y = Mathf.Round(@this.y * power) / power;
            @this.z = Mathf.Round(@this.z * power) / power;
            return @this;
        }

        public static Vector4 RoundTo(this Vector4 @this, int decimals)
        {
            var power = 10.Pow(decimals);
            @this.x = Mathf.Round(@this.x * power) / power;
            @this.y = Mathf.Round(@this.y * power) / power;
            @this.z = Mathf.Round(@this.z * power) / power;
            @this.w = Mathf.Round(@this.w * power) / power;
            return @this;
        }

        public static void RoundTo(this Transform @this, int decimals)
        {
            @this.localPosition = @this.localPosition.RoundTo(decimals);
            @this.localEulerAngles = @this.localEulerAngles.RoundTo(decimals);
            @this.localScale = @this.localScale.RoundTo(decimals);
        }

        public static float GetHue(this Color @this)
        {
            return System.Drawing.Color.FromArgb(
                (byte) (@this.a * 256),
                (byte) (@this.r * 256),
                (byte) (@this.g * 256),
                (byte) (@this.b * 256)
            ).GetHue();
        }

        public static void Select(this Object @this)
        {
            var e = UnityEngine.EventSystems.EventSystem.current;
            e.SetSelectedGameObject(@this.GetGameObject());
        }

        public static bool IsSelected(this Object @this)
        {
            var e = UnityEngine.EventSystems.EventSystem.current;
            return e && e.currentSelectedGameObject == @this.GetGameObject();
        }

        public static bool IsInteractableRecursive(this GameObject @this)
        {
            if (!@this)
                return false;

            if (!@this.TryGetComponent(out UnityEngine.UI.Selectable selectable))
                return false;

            if (!selectable.isActiveAndEnabled)
                return false;

            if (!selectable.IsInteractableRecursive())
                return false;

            return true;
        }

        public static bool IsInteractableRecursive(this UnityEngine.UI.Selectable @this)
        {
            if (!@this.interactable)
                return false;

            var canvasGroups = new List<CanvasGroup>();
            var result = true;

            var transform = @this.transform;
            while (transform)
            {
                transform.GetComponents(canvasGroups);
                var ignoreParentGroups = false;

                for (int i = 0, count = canvasGroups.Count; i < count; i++)
                {
                    var canvasGroup = canvasGroups[i];
                    result &= canvasGroup.interactable;
                    ignoreParentGroups |= canvasGroup.ignoreParentGroups || !canvasGroup.interactable;
                }

                if (ignoreParentGroups)
                    break;

                transform = transform.parent;
            }

            return result;
        }

        public static string GetPath(this Transform @this)
        {
            var path = @this.name;
            while (@this.parent)
            {
                @this = @this.parent;
                path = $"{@this.name}/{path}";
            }
            return path;
        }

#if UNITY_EDITOR
        static readonly Texture2D[] labelBackgrounds = new Texture2D[9];

        public struct EditorLabelOptions
        {
            public Vector3 offset;
        }

        public static void DrawEditorLabel(
            this MonoBehaviour @this,
            string text,
            EditorLabelOptions options = default
        )
        {
            @this.DrawEditorLabel(default, text, options);
        }

        public static void DrawEditorLabel(
            this MonoBehaviour @this,
            string title,
            string text,
            EditorLabelOptions options = default
        )
        {
            @this.DrawEditorLabel(title, _ => text, options);
        }

        public static void DrawEditorLabel(
            this MonoBehaviour @this,
            string title,
            Func<string, string> getText,
            EditorLabelOptions options = default
        )
        {
            var sceneView = SceneView.currentDrawingSceneView;
            if (!sceneView)
                return;

            if (@this.gameObject.scene != SceneManager.GetActiveScene())
                return;

            for (var i = 0; i < 9; i++)
            {
                if (!labelBackgrounds[i])
                {
                    labelBackgrounds[i] = new(1, 1);
                    var color = new Color(0, 0, 0, ((i + 1) / 10f));
                    labelBackgrounds[i].SetPixel(0, 0, color);
                    labelBackgrounds[i].Apply();
                }
            }

            var distance = Mathf.Max(12, sceneView.cameraDistance);
            var use3dIcons = GizmoUtility.use3dIcons;
            var index = 8 - Mathf.RoundToInt(
                Mathf.Clamp((distance - 12) / (use3dIcons ? 12 : 3), 0, 8)
            );

            var labelBackground = labelBackgrounds[index];

            var contentText = string.Empty;

            var alpha = index / 8f;
            if (alpha > 0)
            {
                var a = alpha.ToHex();
                if (!title.IsNullOrEmpty())
                    contentText = $"<color=#ffff00{a}>{title}</color>";

                var text = getText(a);
                if (!text.IsNullOrEmpty())
                {
                    if (contentText != string.Empty)
                        contentText += '\n';
                    contentText += string.Join('\n', text);
                }
            }

            float scale;

            if (use3dIcons)
            {
                var iconSize = GizmoUtility.iconSize;
                if (iconSize == 0)
                    return;

                scale = 512 / distance * iconSize;
            }
            else
            {
                scale = .75f;
            }

            var fontSize = Mathf.CeilToInt(20 * scale);
            scale = fontSize / 12f;

            var position = @this.transform.position + options.offset;
            var isAbove = options.offset.y > 0;

            var horizontalPadding = Mathf.CeilToInt(scale * 6);
            var verticalPadding = Mathf.CeilToInt(scale * 4);
            var style = new GUIStyle
            {
                font = Utils.GetResource<Font>("Rubik"),
                fontSize = fontSize,
                alignment = isAbove
                    ? TextAnchor.LowerCenter
                    : TextAnchor.UpperCenter,
                wordWrap = true,
                padding = new(
                    horizontalPadding,
                    horizontalPadding,
                    verticalPadding,
                    verticalPadding
                ),
                normal = new()
                {
                    textColor = new(1, 1, 1, alpha),
                    background = labelBackground
                }
            };

            var content = new GUIContent(contentText);

            var maxWidth = 240 * scale;
            var h = style.CalcHeight(content, maxWidth + 8);
            var w = Mathf.Min(style.CalcSize(content).x, maxWidth) + 8;

            Handles.BeginGUI();
            var p = HandleUtility.WorldToGUIPoint(position);
            GUI.Label(
                new(p.x - w / 2, p.y - (isAbove ? h : 0), w, h),
                content,
                style
            );
            Handles.EndGUI();
        }

        public static string ToHex(this float @this)
        {
            return ((int) (255 * @this)).ToString("X");
        }
#endif

        public static bool IsNullOrEmpty(this string @this)
        {
            return @this is null || @this == string.Empty;
        }

        public static IEnumerable<T> EnumerateSceneObjectsByType<T>(this Object @object) where T : Object
        {
            var scene = @object.GetGameObject().scene;
            return Object
                .FindObjectsByType<T>(FindObjectsSortMode.None)
                .Where(x => x.GetGameObject().scene == scene);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n-- > 1)
            {
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}

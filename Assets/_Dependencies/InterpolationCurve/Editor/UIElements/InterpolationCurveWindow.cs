using Prototype.Editor;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Flags = System.Reflection.BindingFlags;

namespace InterpolationCurve.Editor.UIElements
{
    public class InterpolationCurveWindow : EditorWindow, IAreaProvider
    {
        static InterpolationCurveWindow _window;
        static InterpolationCurveView _view;
        static TimelineBar _timelineBar;

        static System.Reflection.MethodInfo internalEvaluateMethod
            = typeof(InterpolationCurve).GetMethod("InternalEvaluate", Flags.NonPublic | Flags.Instance);

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var @object = EditorUtility.InstanceIDToObject(instanceID);
            if (@object is InterpolationCurve interpolationCurve)
            {
                Open(interpolationCurve);
            }
            return false;
        }

        public static void Open(InterpolationCurve interpolationCurve)
        {
            if (!interpolationCurve)
            {
                return;
            }
            _window = GetWindow<InterpolationCurveWindow>(true, interpolationCurve.name, true);
            _window.position = Utils.CalcPositionByMousePosition(720, 360);
            _window.assetEditingWhileRuntime = Application.isPlaying && !interpolationCurve.info.assetObject;
            _window.interpolationCurveObject = new SerializedObject(interpolationCurve);
            _window.RefreshLayout();
        }

        SerializedObject interpolationCurveObject;
        FloatField minField;
        FloatField maxField;
        EnumField loopTypeField;
        EvaluationSlider evaluationSlider;
        bool assetEditingWhileRuntime;

        void OnDestroy()
        {
            if (!assetEditingWhileRuntime)
                return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            var flags = Flags.Instance | Flags.Public | Flags.NonPublic;
            foreach (var component in gameObjects.SelectMany(x => x.GetComponents<Component>()))
            {
                var fields = component.GetType().GetFields(flags).Where(x => x.FieldType == typeof(InterpolationCurve)).ToArray();
                foreach (var field in fields)
                {
                    var interpolationCurve = field.GetValue(component) as InterpolationCurve;
                    var assetObject = interpolationCurve ? interpolationCurve.info.assetObject : null;
                    if (assetObject)
                        field.SetValue(component, assetObject.ToRuntimeObject());
                }
            }
        }

        void OnLostFocus()
        {
            Close();
        }

        void AddToolbar()
        {
            var toolbar = new Toolbar();

            var interpolationCurve = interpolationCurveObject?.targetObject as InterpolationCurve;
            var assetObject = interpolationCurve.info.assetObject;
            var interpolationCurveField = new Button(() =>
            {
                if (assetObject)
                {
                    EditorGUIUtility.PingObject(assetObject);
                    assetEditingWhileRuntime = true;
                    interpolationCurveObject = new SerializedObject(assetObject);
                    RefreshLayout();
                    return;
                }

                EditorGUIUtility.PingObject(interpolationCurve);
            });

            if (interpolationCurve)
            {
                if (assetObject)
                {
                    interpolationCurveField.style.color = Color.yellow;
                    interpolationCurveField.text = "Edit Asset";
                }
                else
                {
                    interpolationCurveField.style.color = Color.white;
                    interpolationCurveField.text = "Find Asset in Project";
                }
            }
            else
            {
                interpolationCurveField.text = "(None)";
                interpolationCurveField.SetEnabled(false);
            }
            toolbar.Add(interpolationCurveField);

            minField = new FloatField("Min");
            minField.Q<Label>().style.width = 50;
            minField.Q<Label>().style.minWidth = 50;
            minField.Q<Label>().style.unityTextAlign = TextAnchor.MiddleRight;
            minField.Q<Label>().style.marginRight = 5;
            minField.style.width = 100;
            minField.style.height = 20;
            minField.style.marginTop = 4;
            if (interpolationCurveObject == null)
            {
                minField.SetEnabled(false);
            }
            else
            {
                minField.BindProperty(interpolationCurveObject.FindProperty("min"));
            }
            toolbar.Add(minField);

            maxField = new FloatField("Max");
            maxField.Q<Label>().style.width = 50;
            maxField.Q<Label>().style.minWidth = 50;
            maxField.Q<Label>().style.unityTextAlign = TextAnchor.MiddleRight;
            maxField.Q<Label>().style.marginRight = 5;
            maxField.style.width = 100;
            maxField.style.height = 20;
            maxField.style.marginTop = 4;

            if (interpolationCurveObject is null)
                maxField.SetEnabled(false);
            else
                maxField.BindProperty(interpolationCurveObject.FindProperty("max"));

            toolbar.Add(maxField);

            evaluationSlider = new EvaluationSlider();
            toolbar.Add(evaluationSlider);

            rootVisualElement.Add(toolbar);
        }

        void AddTimelineBar()
        {
            if (interpolationCurveObject is null)
                return;

            _timelineBar = new(this);
            rootVisualElement.Add(_timelineBar);
        }

        void Update()
        {
            if (_view is not null)
            {
                if (interpolationCurveObject is not null)
                {
                    var interpolationCurve = interpolationCurveObject.targetObject as InterpolationCurve;
                    var t = evaluationSlider.value;
                    var v = (float) internalEvaluateMethod.Invoke(interpolationCurve, new object[] { t });
                    _view.evaluationElement.position = new(t, v);
                }
                _view.Update();
            }
            _timelineBar?.Update();
        }

        void RefreshLayout()
        {
            rootVisualElement.Clear();

            rootVisualElement.styleSheets.Add(
                Prototype.Utils.LoadAsset<StyleSheet>(nameof(InterpolationCurveWindow))
            );

            _view = new InterpolationCurveView(this);
            _view.RegisterCallback<MouseDownEvent>(e =>
            {
                _timelineBar.propertiesElement.eventElement = null;
            });
            _view.StretchToParentSize();
            rootVisualElement.Add(_view);
            AddToolbar();
            AddTimelineBar();

            var layoutAsset = Prototype.Utils.LoadAsset<VisualTreeAsset>(nameof(InterpolationCurveWindow));

            VisualElement layoutElement = layoutAsset.CloneTree();
            rootVisualElement.Add(layoutElement);

            if (interpolationCurveObject != null)
            {
                _view.Refresh(interpolationCurveObject);
                _timelineBar.Refresh(interpolationCurveObject);
            }
        }

        public Vector2 GetAbsolute(Vector2 relative) => new(
            Mathf.Lerp(20, position.width - 20, relative.x),
            Mathf.Lerp(50, position.height - 55, relative.y)
        );

        public Vector2 GetRelative(Vector2 absolute) => new(
            Mathf.Clamp01((absolute.x - 20) / (position.width - 40)),
            Mathf.Clamp01(1 - (absolute.y - 20) / (position.height - 105))
        );
    }
}

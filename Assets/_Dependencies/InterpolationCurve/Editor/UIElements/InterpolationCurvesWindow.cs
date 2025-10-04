using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class InterpolationCurvesWindow : EditorWindow
    {
        const int WIDTH = 600;
        const int HEIGHT = 400;

        SerializedProperty property;
        InterpolationCurve[] interpolationCurves;

        public static InterpolationCurvesWindow Create(SerializedProperty property)
        {
            var window = GetWindow<InterpolationCurvesWindow>(true, "Interpolation Curves", true);
            window.position = Utils.CalcPositionByMousePosition(WIDTH, HEIGHT);
            window.property = property;

            var guids = AssetDatabase.FindAssets($"t:{typeof(InterpolationCurve).FullName}");
            var paths = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
            window.interpolationCurves = paths.Select(x => AssetDatabase.LoadAssetAtPath<InterpolationCurve>(x)).ToArray();

            window.Refresh();
            return window;
        }

        void Refresh()
        {
            rootVisualElement.Clear();

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.paddingTop = 10;
            scrollView.style.paddingRight = 10;
            scrollView.style.paddingBottom = 10;
            scrollView.style.paddingLeft = 10;
            rootVisualElement.Add(scrollView);

            var rows = new VisualElement[Mathf.CeilToInt((interpolationCurves.Length + 2) / 2f)];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new VisualElement();
                rows[i].style.flexDirection = FlexDirection.Row;
                scrollView.Add(rows[i]);
            }

            rows[0].Add(CreateInterpolationCurveElement(null, false));

            int _i;
            for (_i = 0; _i < interpolationCurves.Length; _i++)
                rows[(_i + 1) / 2].Add(CreateInterpolationCurveElement(interpolationCurves[_i], false));
            rows[(_i + 1) / 2].Add(CreateInterpolationCurveElement(null, true));
        }

        IMGUIContainer CreateInterpolationCurveElement(InterpolationCurve interpolationCurve, bool createNew)
        {
            var width = WIDTH / 2 - 15;
            var height = HEIGHT / 6;
            var element = new IMGUIContainer(() =>
            {
                Utils.DrawInterpolationCurve(new Rect(5, 5, width - 10, height - 10), interpolationCurve, createNew);
            })
            {
                userData = new object[] { interpolationCurve, createNew }
            };
            element.style.borderTopWidth = 1;
            element.style.borderRightWidth = 1;
            element.style.borderBottomWidth = 1;
            element.style.borderLeftWidth = 1;
            Leave(element);
            element.RegisterCallback<MouseEnterEvent>(e => Enter(e.target as VisualElement));
            element.RegisterCallback<MouseLeaveEvent>(e => Leave(e.target as VisualElement));
            element.RegisterCallback<MouseDownEvent>(e =>
            {
                var @object = property.objectReferenceValue as InterpolationCurve;
                var isRuntimeObject = @object && @object.info.assetObject;

                if (interpolationCurve)
                {
                    if (isRuntimeObject)
                        interpolationCurve = interpolationCurve.ToRuntimeObject();

                    property.objectReferenceValue = interpolationCurve;
                    Utils.CreateBezierPoints(interpolationCurve);
                    property.serializedObject.ApplyModifiedProperties();
                    Close();
                    return;
                }

                if (createNew)
                {
                    var newInterpolationCurve = Utils.Create<InterpolationCurve>("New Interpolation Curve");
                    if (isRuntimeObject)
                        newInterpolationCurve = newInterpolationCurve.ToRuntimeObject();

                    property.objectReferenceValue = newInterpolationCurve;
                    Utils.CreateBezierPoints(newInterpolationCurve);
                    property.serializedObject.ApplyModifiedProperties();
                    Close();
                    InterpolationCurveWindow.Open(newInterpolationCurve);
                    return;
                }

                if (isRuntimeObject)
                {
                    Debug.LogWarning("A runtime object cannot be set to null");
                }
                else
                {
                    property.objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                Close();
            });
            element.style.width = width;
            element.style.height = height;
            return element;
        }

        void Enter(VisualElement element)
        {
            element.style.borderTopColor = Color.white;
            element.style.borderRightColor = Color.white;
            element.style.borderBottomColor = Color.white;
            element.style.borderLeftColor = Color.white;
        }

        void Leave(VisualElement element)
        {
            var data = element.userData as object[];
            var userInterpolationCurve = data[0] as InterpolationCurve;
            if (userInterpolationCurve)
            {
                var current = property.objectReferenceValue as InterpolationCurve;
                if (userInterpolationCurve == current || (current && current.info.assetObject == userInterpolationCurve))
                    SetActive(element);
                else
                    SetInactive(element);
            }
            else
            {
                var userCreateNew = (bool) data[1];
                if (!userCreateNew && !property.objectReferenceValue)
                    SetActive(element);
                else
                    SetInactive(element);
            }
        }

        void SetActive(VisualElement element)
        {
            element.style.borderTopColor = Color.cyan;
            element.style.borderRightColor = Color.cyan;
            element.style.borderBottomColor = Color.cyan;
            element.style.borderLeftColor = Color.cyan;
        }

        void SetInactive(VisualElement element)
        {
            element.style.borderTopColor = Color.clear;
            element.style.borderRightColor = Color.clear;
            element.style.borderBottomColor = Color.clear;
            element.style.borderLeftColor = Color.clear;
        }

        void OnLostFocus()
        {
            Close();
        }
    }
}

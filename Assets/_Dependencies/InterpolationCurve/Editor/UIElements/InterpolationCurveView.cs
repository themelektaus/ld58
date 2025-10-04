using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class InterpolationCurveView : VisualElement
    {
        SerializedObject interpolationCurveObject;
        IAreaProvider areaProvider;

        readonly List<PointContainer> pointContainers = new List<PointContainer>();
        readonly List<ConnectionInfo> connectionInfos = new List<ConnectionInfo>();

        PointContainer draggingPosition;
        PointContainer draggingInTangent;
        PointContainer draggingOutTangent;

        Vector2 mousePosition;
        bool ctrl;
        bool shift;
        bool alt;
        bool ignoreNextMouseUp;

        readonly Grid grid;

        public readonly EvaluationElement evaluationElement;

        public InterpolationCurveView(IAreaProvider areaProvider)
        {
            this.areaProvider = areaProvider;

            name = "InterpolationCurveView";

            grid = new Grid();
            evaluationElement = new EvaluationElement();

            RegisterCallback<MouseMoveEvent>(e =>
            {
                mousePosition = e.localMousePosition;
                ctrl = e.ctrlKey;
                shift = e.shiftKey;
                alt = e.altKey;
            });

            RegisterCallback<MouseUpEvent>(e =>
            {
                if (ignoreNextMouseUp)
                {
                    ignoreNextMouseUp = false;
                    return;
                }

                var interpolationCurve = interpolationCurveObject.targetObject as InterpolationCurve;

                if (draggingPosition != null || draggingInTangent != null || draggingOutTangent != null)
                {
                    UpdateBezierPoints(interpolationCurve);
                    draggingPosition = null;
                    draggingInTangent = null;
                    draggingOutTangent = null;
                    return;
                }

                if (e.button == 1)
                {
                    interpolationCurve.points.Add(new InterpolationCurve.Point
                    {
                        position = areaProvider.GetRelative(mousePosition),
                        tangent = new Vector4(-0.1f, 0, 0.1f, 0)
                    });
                    interpolationCurveObject.Update();
                    Refresh();
                    return;
                }
            });
        }

        void UpdateBezierPoints(InterpolationCurve interpolationCurve)
        {
            if (
                draggingPosition is not null ||
                draggingInTangent is not null ||
                draggingOutTangent is not null
            )
                interpolationCurveObject.ApplyModifiedProperties();

            Utils.CreateBezierPoints(interpolationCurve);
            interpolationCurveObject.Update();
        }

        public void Refresh()
        {
            Refresh(interpolationCurveObject);
        }

        public void Refresh(SerializedObject interpolationCurveObject)
        {
            Clear();

            this.interpolationCurveObject = interpolationCurveObject;

            pointContainers.Clear();
            connectionInfos.Clear();

            draggingPosition = null;
            draggingInTangent = null;
            draggingOutTangent = null;

            if (this.interpolationCurveObject == null)
            {
                return;
            }

            Add(grid);
            grid.StretchToParentSize();

            var pointsProperty = this.interpolationCurveObject.FindProperty("points");

            for (int i = 0; i < pointsProperty.arraySize; i++)
            {
                var pointContainer = new PointContainer(pointsProperty.GetArrayElementAtIndex(i), i);
                pointContainer.SetPositionEvent(e =>
                {
                    if (e.button == 1)
                    {
                        var index = (e.target as PositionElement).pointContainer.index;
                        pointsProperty.DeleteArrayElementAtIndex(index);
                        this.interpolationCurveObject.ApplyModifiedProperties();
                        ignoreNextMouseUp = true;
                        Refresh();
                        return;
                    }
                    draggingPosition = pointContainer;
                });

                if (i > 0)
                {
                    pointContainer.AddInTangent(e =>
                    {
                        draggingInTangent = pointContainer;
                    });
                }

                if (i < pointsProperty.arraySize - 1)
                {
                    pointContainer.AddOutTangent(e =>
                    {
                        draggingOutTangent = pointContainer;
                    });
                }

                pointContainers.Add(pointContainer);
            }

            for (int i = 1; i < pointsProperty.arraySize; i++)
            {
                var connectionInfo = new ConnectionInfo
                {
                    connectionElement = new ConnectionElement(),
                    pointContainerA = pointContainers[i - 1],
                    pointContainerB = pointContainers[i]
                };
                connectionInfos.Add(connectionInfo);
                Add(connectionInfo.connectionElement);
            }

            foreach (var pointContainer in pointContainers)
            {
                pointContainer.AddTo(this);
            }

            Add(evaluationElement);
        }

        public void Update()
        {
            if (interpolationCurveObject == null)
            {
                return;
            }

            var interpolationCurve = interpolationCurveObject.targetObject as InterpolationCurve;

            if (draggingPosition != null || draggingInTangent != null || draggingOutTangent != null)
            {
                UpdateBezierPoints(interpolationCurve);
            }

            var points = interpolationCurve.points;
            var sortedPoints = points.OrderBy(x => x.position.x).ToList();
            if (!points.SequenceEqual(sortedPoints))
            {
                interpolationCurve.points = sortedPoints;
                interpolationCurveObject.Update();
                Refresh();
                return;
            }

            grid.Update(areaProvider);

            if (draggingPosition != null)
            {
                var position = areaProvider.GetRelative(mousePosition);
                if (ctrl)
                {
                    position.x = Mathf.Round(position.x * 20) / 20;
                    position.y = Mathf.Round(position.y * 10) / 10;
                }
                draggingPosition.position = position;
            }

            for (int i = 0; i < pointContainers.Count; i++)
            {
                var @this = pointContainers[i];
                var prev = i == 0 ? null : pointContainers[i - 1];
                var next = i == pointContainers.Count - 1 ? null : pointContainers[i + 1];
                var tangentDelta = @this.Update(prev, next);

                if (@this == draggingInTangent)
                {
                    var tangent = @this.tangent;
                    var tangentPosition = areaProvider.GetRelative(mousePosition) - @this.position;
                    tangent.x = tangentPosition.x / tangentDelta.x;
                    tangent.y = tangentPosition.y / tangentDelta.y;
                    if (ctrl)
                    {
                        tangent.x = Mathf.Round(tangent.x * 10) / 10;
                        tangent.y = Mathf.Round(tangent.y * 10) / 10;
                    }
                    if (shift)
                    {
                        tangent.z = -tangent.x;
                        tangent.w = -tangent.y;
                    }
                    @this.tangent = tangent;
                }

                if (@this == draggingOutTangent)
                {
                    var tangent = @this.tangent;
                    var tangentPosition = areaProvider.GetRelative(mousePosition) - @this.position;
                    tangent.z = tangentPosition.x / tangentDelta.z;
                    tangent.w = tangentPosition.y / tangentDelta.w;
                    if (ctrl)
                    {
                        tangent.z = Mathf.Round(tangent.z * 10) / 10;
                        tangent.w = Mathf.Round(tangent.w * 10) / 10;
                    }
                    if (shift)
                    {
                        tangent.x = -tangent.z;
                        tangent.y = -tangent.w;
                    }
                    @this.tangent = tangent;
                }

                @this.UpdateStyle(areaProvider, prev, next);
            }

            foreach (var info in connectionInfos)
            {
                var a = info.pointContainerA.position;
                var b = info.pointContainerB.position;
                info.connectionElement.a = areaProvider.GetAbsolute(a);
                info.connectionElement.b = areaProvider.GetAbsolute(b);
                var aTangent = info.pointContainerA.tangent;
                var bTangent = info.pointContainerB.tangent;
                info.connectionElement.tangent = new Vector4(aTangent.z, aTangent.w, bTangent.x, bTangent.y);
                info.connectionElement.UpdateStyle();
            }

            evaluationElement.Update(
                areaProvider,
                interpolationCurveObject.FindProperty("min").floatValue,
                interpolationCurveObject.FindProperty("max").floatValue
            );
        }
    }
}

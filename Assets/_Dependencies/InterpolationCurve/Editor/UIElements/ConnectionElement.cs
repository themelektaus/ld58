using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace InterpolationCurve.Editor.UIElements
{
    public class ConnectionElement : IMGUIContainer
    {
        public Vector2 a;
        public Vector2 b;
        public Vector4 tangent;
        public bool showBezierPoints;

        public ConnectionElement() : base()
        {
            onGUIHandler = () =>
            {
                var (startPosition, endPosition, startTangent, endTangent) = GetBezier();

                Handles.color = Color.gray;
                Handles.DrawLine(startPosition, startTangent);
                Handles.DrawLine(endPosition, endTangent);
                Handles.DrawBezier(
                    startPosition,
                    endPosition,
                    startTangent,
                    endTangent,
                    new Color(0, 1, 1, .3f),
                    EditorGUIUtility.whiteTexture,
                    4
                );
                Handles.DrawBezier(
                    startPosition,
                    endPosition,
                    startTangent,
                    endTangent,
                    new Color(0, 1, 1, .5f),
                    EditorGUIUtility.whiteTexture,
                    2
                );
                Handles.DrawBezier(
                    startPosition,
                    endPosition,
                    startTangent,
                    endTangent,
                    Color.cyan,
                    EditorGUIUtility.whiteTexture,
                    1
                );
                if (showBezierPoints)
                {
                    var rect = GetRect();
                    foreach (var p in Utils.GetBezierPoints(tangent))
                    {
                        var x = p.x;
                        var y = a.y > b.y ? 1 - p.y : p.y;
                        EditorGUI.DrawRect(new Rect(x * rect.width - 2, (rect.height - y * rect.height) - 2, 4, 4), Color.cyan);
                    }
                }
            };
        }

        public void UpdateStyle()
        {
            var rect = GetRect();
            style.position = Position.Absolute;
            style.left = rect.x;
            style.bottom = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }

        Rect GetRect()
        {
            Vector2 min = Vector2.Min(a, b);
            Vector2 max = Vector2.Max(a, b);
            return new(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        (Vector2 startPosition, Vector2 endPosition, Vector2 startTangent, Vector2 endTangent) GetBezier()
        {
            var rect = GetRect();
            var startPosition = new Vector2(0, a.y > b.y ? 0 : rect.height);
            var endPosition = new Vector2(rect.width, a.y < b.y ? 0 : rect.height);
            return (
                startPosition,
                endPosition,
                startPosition + new Vector2(tangent.x * rect.width, (a.y > b.y ? 1 : -1) * tangent.y * rect.height),
                endPosition + new Vector2(tangent.z * rect.width, (a.y > b.y ? 1 : -1) * tangent.w * rect.height)
            );
        }
    }
}

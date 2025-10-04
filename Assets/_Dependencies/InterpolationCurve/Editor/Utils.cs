using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace InterpolationCurve.Editor
{
    public static class Utils
    {
        public static Rect CalcPositionByMousePosition(float width, float height)
        {
            var mousePoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var position = new Rect(mousePoint.x, mousePoint.y, width, height);
            var halfResolution = DisplayInfo.Resolution / 2;
            var cursorPosition = DisplayInfo.CursorPosition;
            position.x -= cursorPosition.x > halfResolution.x ? position.width + 10 : -10;
            position.y -= cursorPosition.y > halfResolution.y ? position.height + 10 : -50;
            return position;
        }

        public static void DrawInterpolationCurve(Rect position, InterpolationCurve interpolationCurve, bool createNew)
        {
            var color = GUI.color;

            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);

            position.x++;
            position.y++;
            position.width -= 2;
            position.height -= 2;
            GUI.color = new Color(1, 1, 1, 0.2f);
            GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);

            GUI.color = color;

            if (interpolationCurve)
            {
                color = Handles.color;
                Handles.color = Color.cyan;

                var lineOffset = new Vector2(0, 1);

                var bezierPoints = interpolationCurve.bezierPoints;
                for (int i = 1; i < bezierPoints.Count; i++)
                {
                    var a = new Vector2(
                        position.x + position.width * bezierPoints[i - 1].x,
                        position.y + position.height * (1 - bezierPoints[i - 1].y)
                    );

                    var b = new Vector2(
                        position.x + position.width * bezierPoints[i].x,
                        position.y + position.height * (1 - bezierPoints[i].y)
                    );

                    Handles.DrawLine(a, b);
                    Handles.DrawLine(a + lineOffset / 2, b + lineOffset / 2);
                    Handles.DrawLine(a + lineOffset, b + lineOffset);
                }

                Handles.color = color;

                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(4, 4, -1, 1),
                    margin = new RectOffset()
                };

                var labelContent = new GUIContent(interpolationCurve.name);
                var labelWidth = labelStyle.CalcSize(labelContent).x;

                var labelPosition = position;
                labelPosition.height = 12;
                labelPosition.width = labelWidth;
                EditorGUI.DrawRect(labelPosition, new Color(0, 0.2f, 0.2f, 0.7f));

                GUI.Label(position, labelContent, labelStyle);
                return;
            }

            GUI.Label(position, createNew ? "(New Interpolation Curve)" : "(None)", new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            });
        }

        public static T GetObject<T>(SerializedProperty property) where T : Object
        {
            return property.objectReferenceValue as T;
        }

        public static T Create<T>(string assetName) where T : ScriptableObject
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/{assetName}.asset");
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void Open(SerializedProperty property)
        {
            Open(property.objectReferenceValue);
        }

        public static void Open(Object obj)
        {
            var selection = Selection.activeObject;
            AssetDatabase.OpenAsset(obj);
            Selection.activeObject = selection;
        }

        static int objectPickerControlID;

        public static void OpenObjectPicker<T>(SerializedProperty property) where T : Object
        {
            OpenObjectPicker(property.objectReferenceValue as T);
        }

        public static void OpenObjectPicker<T>(T obj) where T : Object
        {
            objectPickerControlID = GUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<T>(obj, false, null, objectPickerControlID);
        }

        public static bool TrGetObjectPickerSelection<T>(out T obj) where T : Object
        {
            obj = null;
            if (Event.current.commandName != "ObjectSelectorUpdated")
                return false;

            var controlID = EditorGUIUtility.GetObjectPickerControlID();
            if (objectPickerControlID != controlID)
                return false;

            obj = EditorGUIUtility.GetObjectPickerObject() as T;
            return true;
        }

        public static bool IsMouseDown(Rect position)
        {
            var e = Event.current;
            
            if (e.type == EventType.MouseDown && position.Contains(e.mousePosition))
                return true;

            return false;
        }

        public static IEnumerable<Vector2> GetBezierPoints(Vector4 tangent)
        {
            var startPosition = new Vector2(0, 0);
            var endPosition = new Vector2(1, 1);

            var startTangent = startPosition + new Vector2(tangent.x, tangent.y);
            var endTangent = endPosition + new Vector2(tangent.z, tangent.w);

            foreach (var point in Handles.MakeBezierPoints(
                startPosition,
                endPosition,
                startTangent,
                endTangent,
                20
            ))
            {
                yield return point;
            }
        }

        public static void CreateBezierPoints(InterpolationCurve interpolationCurve)
        {
            interpolationCurve.bezierPoints.Clear();
            for (int i = 1; i < interpolationCurve.points.Count; i++)
            {
                var startPoint = interpolationCurve.points[i - 1];
                var endPoint = interpolationCurve.points[i];
                var a = startPoint.position;
                var b = endPoint.position;
                var tangent = new Vector4(
                    startPoint.tangent.z,
                    startPoint.tangent.w,
                    endPoint.tangent.x,
                    endPoint.tangent.y
                );

                foreach (var bezierPoint in GetBezierPoints(tangent))
                    interpolationCurve.bezierPoints.Add(a + bezierPoint * (b - a));
            }
        }
    }
}

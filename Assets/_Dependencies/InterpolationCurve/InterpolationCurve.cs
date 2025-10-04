using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace InterpolationCurve
{
    public sealed class InterpolationCurve : ScriptableObject
    {
        public sealed class Info
        {
            public InterpolationCurve assetObject;
            public bool runtimeWarning;
            public int frame;
            public float currentTime;
            public float lastTime = -1;
        }

        public Info info { get; private set; } = new Info();

        public float min = 0;
        public float max = 1;

        public event System.Action<string> onTrigger;

        [System.Serializable]
        public struct Point
        {
            public Vector2 position;
            public Vector4 tangent;
        }

        public List<Point> points = new()
        {
            new Point { tangent = new Vector4(-.1f, 0, .1f, 0) },
            new Point { tangent = new Vector4(-.1f, 0, .1f, 0), position = Vector2.one }
        };

        public List<Vector2> bezierPoints = new();

        [System.Serializable]
        public class Timeline
        {
            [System.Serializable]
            public class Event
            {
                public float time;
                public string name;
            }

            public List<Event> events = new();
        }
        public Timeline timeline = new();

        public bool isRuntimeObject => info.assetObject;

        public InterpolationCurve ToRuntimeObject()
        {
            if (isRuntimeObject)
            {
                Debug.LogWarning("The interpolation curve is already an runtime object", this);
                return this;
            }

            var result = Instantiate(this);
            result.name = name + " (Runtime Object)";
            result.info.assetObject = this;
            return result;
        }

        public float Evaluate(float t)
        {
            t = Mathf.Clamp01(t);

            if (isRuntimeObject)
                info.currentTime = t;

            EvaluateTriggers();

            return min + InternalEvaluate(t) * (max - min);
        }

        void EvaluateTriggers()
        {
            if (onTrigger is null)
                return;

            if (!isRuntimeObject)
            {
                if (!info.runtimeWarning)
                {
                    info.runtimeWarning = true;
                    Debug.LogWarning("You only can get triggered events from a runtime instance", this);
                }
                return;
            }

            var frame = Time.frameCount;

            if (info.frame < frame)
            {
                info.frame = frame;

                if (0 <= info.lastTime && info.lastTime < info.currentTime)
                    foreach (var @event in timeline.events)
                        if (info.lastTime < @event.time && @event.time <= info.currentTime)
                            onTrigger.Invoke(@event.name);

                info.lastTime = info.currentTime;
            }
        }

        float InternalEvaluate(float t)
        {
            if (bezierPoints.Count == 0)
                return 0;

            var first = bezierPoints[0];
            if (bezierPoints.Count == 1 || Mathf.Approximately(t, 0))
                return first.y;

            var last = bezierPoints[^1];

            if (Mathf.Approximately(t, 1))
                return last.y;

            foreach (var b in bezierPoints.Where(p => p.x >= t))
            {
                var index = bezierPoints.IndexOf(b);
                if (index == 0)
                    return first.y;

                var a = bezierPoints[index - 1];
                return Mathf.Lerp(a.y, b.y, (t - a.x) / (b.x - a.x));
            }

            return last.y;
        }
    }
}

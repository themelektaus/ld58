using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.DataRenderer2D
{
    [Serializable]
    public partial struct Spline : IEnumerable<Point>
    {
        public enum LineMode
        {
            SplineMode,
            BezierMode
        }

        [SerializeField] LineMode mode;
        [SerializeField] LinePair pair;
        [SerializeField] List<Point> points;

        public event Action EditCallBack;
        public MonoBehaviour owner;

        public LineOption option;

        public static Spline Default => new()
        {
            points = new List<Point>() {
                Point.Zero,
                new(Vector3.right * 10, Vector3.zero, Vector3.zero)
            },
            mode = LineMode.SplineMode,
            pair = new LinePair(Point.Zero, new(Vector3.right, Vector3.zero, Vector3.zero), 0, 1, 0, 1),
            option = LineOption.Default
        };

        public Point GetFirstPoint()
        {
            if (mode == LineMode.BezierMode)
                return pair.n0;

            if (points.Count < 1)
                throw new Exception("need more point");

            return points[0];
        }

        public Point GetLastPoint()
        {
            if (mode == LineMode.BezierMode)
                return pair.n1;

            if (points.Count < 1)
                throw new Exception("need more point");

            return points[^1];
        }

        int GetCount()
        {
            return mode == LineMode.BezierMode ? 2 : points.Count;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            if (mode == LineMode.BezierMode)
            {
                yield return pair.n0;
                yield return pair.n1;
                yield break;
            }

            foreach (var p in points)
                yield return p;
        }

        public IEnumerable<Point> TripleEnumerator()
        {
            if (mode == LineMode.BezierMode)
            {
                yield return pair.n0;
                yield return pair.n1;
                yield break;
            }

            foreach (var p in points)
                yield return p;

            if (option.mode == LineOption.Mode.Loop)
                yield return points[0];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public float AllLength
        {
            get
            {
                float length = 0;

                foreach (var pair in AllPair)
                    length += CurveLength.Auto(pair[0], pair[1]);

                return length;
            }
        }
    }
}
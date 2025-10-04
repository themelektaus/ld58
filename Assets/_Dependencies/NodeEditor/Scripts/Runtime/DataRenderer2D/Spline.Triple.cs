using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.DataRenderer2D
{
    public partial struct Spline
    {
        public IEnumerable<Triple> TripleList
        {
            get
            {
                if (GetCount() < 2)
                    yield break;

                var mode = option.mode;
                var sr = option.startRatio;
                var er = option.endRatio;
                var color = option.color;

                var l = AllLength;
                var ls = sr * l;
                var le = er * l;
                var c = 0f;

                var fB = Point.Zero;
                var ff = true;
                var sB = Point.Zero;
                var sf = true;

                var index = 0;
                foreach (var p in TripleEnumerator())
                {
                    if (ff)
                    {
                        ff = false;
                        fB = p;
                        continue;
                    }

                    if (sf)
                    {
                        if (mode == LineOption.Mode.Loop && sr == 0f && er == 1f)
                            yield return new Triple(GetLastPoint(), GetFirstPoint(), p, color.Evaluate(0));

                        sf = false;
                        sB = p;
                        continue;
                    }

                    c += CurveLength.Auto(fB, sB);

                    if (ls < c && c < le)
                    {
                        if (index == GetCount() - 1 && mode != LineOption.Mode.Loop)
                            break;

                        yield return new Triple(fB, sB, p, color.Evaluate(c / l));
                    }

                    fB = sB;
                    sB = p;
                    index++;
                }
            }
        }

        public struct Triple
        {
            Point previous;
            Point target;
            Point next;
            Color color;

            public Vector3 ForwardDirection => Curve.AutoDirection(target, next, 0f);
            public Vector3 BackDirection => Curve.AutoDirection(previous, target, 1f);
            public Vector3 Position => target.position;
            public float CurrentWidth => target.width;
            public Color CurrentColor => color;

            public Triple(Point p, Point c, Point n, Color cl)
            {
                previous = p; target = c; next = n; color = cl;
            }
        }
    }
}
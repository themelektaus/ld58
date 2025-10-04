using UnityEngine;
using System;

namespace NodeEditor.DataRenderer2D
{
    public partial struct Spline
    {
        public void Initialize()
        {
            this = Default;
        }

        public void Push(Point p)
        {
            if (mode == LineMode.BezierMode)
                throw new Exception("can't add");

            p.position = owner.transform.InverseTransformPoint(p.position);

            points.Add(p);

            EditCallBack?.Invoke();
        }

        public void Push()
        {
            Push(Point.Zero);
        }

        public void Push(Vector3 worldPosition, Vector3 nextOffset, Vector3 prevOffset, float width)
        {
            Push(new Point(worldPosition, nextOffset, prevOffset, width));
        }

        public void EditPoint(int idx, Point p)
        {
            if (mode == LineMode.BezierMode && (idx < 0 || idx > 2))
                throw new Exception("can't edit");

            if (points.Count <= idx || idx < 0)
                throw new Exception("can't edit" + points.Count + " " + idx);

            p.position = owner.transform.InverseTransformPoint(p.position);

            if (mode == LineMode.BezierMode)
            {
                if (idx == 0)
                    pair.n0 = p;
                else
                    pair.n1 = p;
            }
            else
            {
                points[idx] = p;
            }

            EditCallBack?.Invoke();
        }

        public void EditPoint(Vector3 worldPos)
        {
            EditPoint(points.Count - 1, new Point(worldPos, Vector3.zero, Vector3.zero));
        }

        public void EditPoint(int idx, Vector3 worldPos, Vector3 nOffset, Vector3 pOffset, float width)
        {
            EditPoint(idx, new Point(worldPos, nOffset, pOffset, width));
        }

        public void EditPoint(int idx, Vector3 worldPos, float width)
        {
            EditPoint(idx, worldPos, Vector3.zero, Vector3.zero, width);
        }

        public Point Pop()
        {
            if (mode == LineMode.BezierMode)
                throw new Exception("can't remove");

            var last = points[points.Count - 1];
            points.RemoveAt(points.Count - 1);

            EditCallBack?.Invoke();

            return last;
        }

        public int Count => mode == LineMode.BezierMode? 2 : points.Count;

        public void Clear()
        {
            points.Clear();

            EditCallBack?.Invoke();
        }

    }
}
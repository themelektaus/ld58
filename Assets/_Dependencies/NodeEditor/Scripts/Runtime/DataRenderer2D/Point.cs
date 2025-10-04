using System;
using UnityEngine;

namespace NodeEditor.DataRenderer2D
{
    [Serializable]
    public struct Point
    {
        public Vector3 position;
        public Vector3 previousControlOffset;
        public Vector3 nextControlOffset;

        [Range(0, 100)] public float width;

        public Point(Vector3 pos, Vector3 next, Vector3 prev, float width = 2)
        {
            position = pos;
            previousControlOffset = prev;
            nextControlOffset = next;

            this.width = width;
        }

        public Vector3 PreviousControlPoisition => previousControlOffset + position;

        public Vector3 NextControlPosition => nextControlOffset + position;

        public static Point Zero => new(new(), new(), new());
    }
}
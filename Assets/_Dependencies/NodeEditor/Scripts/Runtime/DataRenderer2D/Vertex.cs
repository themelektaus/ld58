using UnityEngine;

namespace NodeEditor.DataRenderer2D
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Color color;

        public Vertex(Vector3 pos, Vector2 u, Color c)
        {
            position = pos;
            uv = u;
            color = c;
        }

        public static Vertex New(Vector3 pos, Vector2 uv, Color color)
        {
            return new Vertex(pos, uv, color);
        }
    }
}
using System.Collections.Generic;

namespace NodeEditor.DataRenderer2D
{
    public struct Quad : IMesh
    {
        Vertex _p0;
        Vertex _p1;
        Vertex _p2;
        Vertex _p3;

        public Quad(Vertex p0, Vertex p1, Vertex p2, Vertex p3)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
        }

        public IEnumerable<Vertex> Vertices
        {
            get
            {
                yield return _p0;
                yield return _p1;
                yield return _p2;
                yield return _p3;

            }
        }

        public IEnumerable<int> Triangles
        {
            get
            {
                var list = new int[] { 0, 2, 1, 1, 2, 3 };
                foreach (var number in list)
                    yield return number;
            }
        }
    }
}
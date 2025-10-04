using System.Collections.Generic;

namespace NodeEditor.DataRenderer2D
{
    public interface IMesh
    {
        IEnumerable<Vertex> Vertices { get; }
        IEnumerable<int> Triangles { get; }
    }
    
    public interface IMeshDrawer
    {
        IEnumerable<IMesh> Draw();
    }

    public interface IJointBuilder
    {
        IEnumerable<IMesh> Build(Spline.Triple triple);
    }

    public interface IBezierBuilder
    {
        IEnumerable<IMesh> Build(Spline.LinePair pair);
    }

    public interface ICapBuilder
    {
        IEnumerable<IMesh> Build(Spline.LinePair pair, bool isEnd);
    }
}
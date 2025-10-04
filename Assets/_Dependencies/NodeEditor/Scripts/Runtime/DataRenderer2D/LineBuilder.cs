using System.Collections.Generic;

namespace NodeEditor.DataRenderer2D
{
    public class LineBuilder : IMeshDrawer
    {
        public static IMeshDrawer CreateNormal(ISpline line) => new LineBuilder(
            new NormalBezierDrawer(line),
            new NormalJointDrawer(line),
            new IntersectJointDrawer(line),
            new RoundCapDrawer(line),
            line
        );

        readonly IBezierBuilder _bezierDrawer;
        readonly IJointBuilder _jointDrawer;
        readonly IJointBuilder _jointIntersectDrawer;
        readonly ICapBuilder _capDrawer;
        readonly ISpline _line;

        public LineBuilder(
            IBezierBuilder bezierDrawer,
            IJointBuilder jointDrawer,
            IJointBuilder jointIntersectDrawer,
            ICapBuilder capBuilder,
            ISpline line
        )
        {
            _bezierDrawer = bezierDrawer;
            _jointDrawer = jointDrawer;
            _jointIntersectDrawer = jointIntersectDrawer;
            _capDrawer = capBuilder;
            _line = line;
        }

        public IEnumerable<IMesh> Draw()
        {
            if (_line.Line.option.endRatio - _line.Line.option.startRatio <= 0)
                yield break;

            var ff = true;

            var last = new Spline.LinePair();

            foreach (var pair in _line.Line.TargetPairList)
            {
                if (ff)
                {
                    ff = false;
                    if (_line.Line.option.mode == LineOption.Mode.RoundEdge)
                        foreach (var mesh in _capDrawer.Build(pair, false))
                            yield return mesh;
                }

                foreach (var mesh in _bezierDrawer.Build(pair))
                    yield return mesh;

                last = pair;
            }

            foreach (var triple in _line.Line.TripleList)
            {
                var joint = _line.Line.option.jointOption == LineOption.LineJointOption.round ? _jointDrawer : _jointIntersectDrawer;

                foreach (var mesh in joint.Build(triple))
                    yield return mesh;
            }

            if (_line.Line.option.mode == LineOption.Mode.RoundEdge)
                foreach (var mesh in _capDrawer.Build(last, true))
                    yield return mesh;
        }
    }
}
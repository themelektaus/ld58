using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NodeEditor.DataRenderer2D
{
    public class RoundCapDrawer : ICapBuilder
    {
        readonly ISpline _line;

        public RoundCapDrawer(ISpline target)
        {
            _line = target;
        }

        public IEnumerable<IMesh> Build(Spline.LinePair pair, bool isEnd)
        {
            var normal = Vector3.back;

            var Line = _line.Line;

            var divideAngle = Line.option.DivideAngle;

            var t = isEnd ? 1f : 0f;

            var color = Line.option.color.Evaluate(isEnd ? Line.option.endRatio : Line.option.startRatio);
            var position = pair.GetPoisition(t);

            var radian = pair.GetWidth(t);

            var direction = pair.GetDirection(isEnd ? 1 : 0) * (isEnd ? -1 : 1);

            var wv = Vector3.Cross(direction, normal).normalized * radian;

            var dc = Mathf.Max(1, Mathf.Floor(180 / divideAngle));
            var da = 180 / dc;
            var rot = Quaternion.Euler(-normal * da);

            var uv = new Vector2[] { new(0, 1), new(1, 1), new(0, 0), new(1, 0) };

            if (_line is Image image && image.sprite)
                uv = image.sprite.uv;

            var center = (uv[0] + uv[1] + uv[2] + uv[3]) / 4;

            for (float a = 0f; a < 179; a += da)
            {
                var v0 = Vertex.New(position, center, color);
                var v1 = Vertex.New(position + wv, (!isEnd ? a > 90 : a < 90) ? uv[1] : uv[0], color);
                var v2 = Vertex.New(position + rot * wv, (!isEnd ? a > 90 : a < 90) ? uv[3] : uv[2], color);

                yield return new Triangle(v0, v1, v2);

                wv = rot * wv;
            }
        }
    }
}
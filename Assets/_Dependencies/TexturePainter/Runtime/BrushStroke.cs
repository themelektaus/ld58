using System.Collections.Generic;
using UnityEngine;

namespace TexturePainter
{
    public struct BrushStroke
    {
        public List<Vector3?> points;
        public Brush brush;
        public bool fill;
    }
}
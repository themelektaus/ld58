using UnityEngine;

namespace TexturePainter
{
    [System.Serializable]
    public struct Brush
    {
        public string property;
        public Color color;

        public void IncreaseAlpha(float value)
        {
            var color = this.color;
            color.a = Mathf.Clamp01(color.a + Mathf.Min(Mathf.Max(0.0005f, color.a * .1f), .05f) * value);
            this.color = color;
        }

        public float size;

        public void IncreaseSize(float value)
        {
			size = Mathf.Max(.00001f, size + Mathf.Clamp(size * .1f, .00001f, .05f) * value);
        }

        public float hardness;

        public void IncreaseHardness(float value)
        {
            hardness = Mathf.Clamp01(hardness + value);
        }
    }
}
using UnityEngine;

namespace TexturePainter
{
    public class TexturePainterConfig : ScriptableObject
    {
        public Brush brush;

        public static TexturePainterConfig Create()
        {
            var config = CreateInstance<TexturePainterConfig>();
            config.name = "Texture Painter Config";
            config.brush.property = "_MainTex";
            config.brush.color = Color.white;
            config.brush.size = .1f;
            return config;
        }
    }
}
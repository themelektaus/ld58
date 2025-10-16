using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace TexturePainter
{
    [CreateAssetMenu]
    public class TexturePainterDataObject : ScriptableObject
    {
        public List<Paintable> paintables = new();

        public Paintable GetPaintable(GameObjectInfo gameObjectInfo)
        {
            var id = gameObjectInfo.GetId();
            return paintables.FirstOrDefault(x => x.id == id);
        }

        public Paintable CreatePaintable(GameObjectInfo gameObjectInfo)
        {
            var paintable = new Paintable { id = gameObjectInfo.GetId() };
            paintables.Add(paintable);
            return paintable;
        }

        public Paintable DeletePaintable(GameObjectInfo gameObjectInfo)
        {
            var paintable = GetPaintable(gameObjectInfo);
            if (paintable is null)
                return null;

#if UNITY_EDITOR
            foreach (var textureInfo in paintable.textureInfos)
                textureInfo.texture.Delete();

            Utils.SaveAssets();
#endif

            paintables.Remove(paintable);

            return paintable;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

using Temp = System.NonSerializedAttribute;

namespace TexturePainter
{
    [System.Serializable]
    public class Paintable
    {
        public long id;
        public List<TextureInfo> textureInfos = new();

        [Temp] PaintableTexture paintableTexture;
#if UNITY_EDITOR
        [Temp] bool shouldSave;
#endif

        public Paintable()
        {
            paintableTexture = null;
#if UNITY_EDITOR
            shouldSave = false;
#endif
        }

#if UNITY_EDITOR
        public void Activate(GameObjectInfo gameObjectInfo, string propertyName)
        {
            paintableTexture ??= new();
            paintableTexture.Activate(this, gameObjectInfo, propertyName);
            shouldSave = true;
        }

        public void Deactivate()
        {
            paintableTexture?.Deactivate(shouldSave);
            shouldSave = false;
        }
#endif

        public bool Update(BrushStroke? brushStroke)
        {
            if (paintableTexture is null)
                return false;

            paintableTexture.Update(brushStroke);
            return true;
        }

        public void PostRender()
        {
            paintableTexture.PostRender();
        }

#if UNITY_EDITOR
        public TextureInfo GetTextureInfo(string propertyName)
        {
            return textureInfos.FirstOrDefault(x => x.propertyName == propertyName);
        }

        public void AddTexture(TexturePainterDataObject dataObject, string gameObjectName, string propertyName, int size, UnityEngine.Color color)
        {
            var textureInfo = new TextureInfo()
            {
                propertyName = propertyName,
                texture = Utils.CreateTexture(gameObjectName + propertyName, size, color)
            };

            dataObject.Add(textureInfo.texture, false);
            textureInfos.Add(textureInfo);

            Utils.SaveAssets();
        }

        public void RemoveTexture(string propertyName)
        {
            var deleted = new List<TextureInfo>();
            foreach (var textureInfo in textureInfos.Where(x => x.propertyName == propertyName))
            {
                textureInfo.texture.Delete();
                deleted.Add(textureInfo);
            }

            foreach (var textureInfo in deleted)
                textureInfos.Remove(textureInfo);

            Utils.SaveAssets();
        }
#endif
    }
}
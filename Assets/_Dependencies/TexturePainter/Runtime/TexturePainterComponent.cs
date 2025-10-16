using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Persistent = UnityEngine.SerializeField;
using Temp = System.NonSerializedAttribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TexturePainter
{
    [ExecuteAlways]
    [AddComponentMenu("Texture Painter Component")]
    public class TexturePainterComponent : MonoBehaviour
    {
        public TexturePainterDataObject dataObject;

        [Persistent] List<GameObjectInfo> gameObjectInfos = new();

        //[Header("Debug")]
        //[Persistent] bool reset;
        //[Persistent] GameObject switchTo;
        //[Persistent] string propertyName = "_MainTex";

        [Temp] GameObjectInfo activeGameObjectInfo;
        [Temp] string activePropertyName;
        [Temp] Paintable activePaintable;

        [Temp] readonly Queue<BrushStroke> brushStrokeQueue = new();

        public GameObjectInfo GetGameObjectInfo(GameObject gameObject) =>
            gameObject ? gameObjectInfos.FirstOrDefault(x => x.GetGameObject() == gameObject) : null;

        public Paintable GetPaintable(GameObject gameObject)
        {
            return GetPaintable(gameObject, out _);
        }

        public Paintable GetPaintable(GameObject gameObject, out GameObjectInfo gameObjectInfo)
        {
            gameObjectInfo = GetGameObjectInfo(gameObject);
            if (gameObjectInfo is null)
                return null;

            return dataObject.GetPaintable(gameObjectInfo);
        }

        public Paintable Register(GameObject gameObject)
        {
            var paintable = GetPaintable(gameObject, out var gameObjectInfo);
            if (paintable is not null)
                return paintable;

            var id = dataObject.paintables.Select(x => x.id).DefaultIfEmpty().Max() + 1;
            gameObjectInfo = GameObjectInfo.Create(id, gameObject);

            gameObjectInfos.Add(gameObjectInfo);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif

            var result = dataObject.CreatePaintable(gameObjectInfo);

            gameObjectInfo.Load(this);

            return result;
        }

        public Paintable Unregister(GameObject gameObject)
        {
            var gameObjectInfo = gameObjectInfos.FirstOrDefault(x => x.GetGameObject() == gameObject);
            if (gameObjectInfo is null)
                return null;

            gameObjectInfo.Unload();

            var result = dataObject.DeletePaintable(gameObjectInfo);

            gameObjectInfos.Remove(gameObjectInfo);

            return result;
        }

        public void SetGameObjectInfo(GameObject gameObject, string propertyName, bool force = false) =>
            SetGameObjectInfo(GetGameObjectInfo(gameObject), propertyName, force);

        public void SetGameObjectInfo(GameObjectInfo gameObjectInfo, string propertyName, bool force)
        {
            if (!force && activeGameObjectInfo == gameObjectInfo && activePropertyName == propertyName)
                return;

            Camera.onPostRender -= PostRender;

            if (activeGameObjectInfo is not null)
                Deactivate();

            if (gameObjectInfo is null)
            {
                Unload();

                activeGameObjectInfo = null;
                activePaintable = null;

                return;
            }

            activeGameObjectInfo = gameObjectInfo;
            activePropertyName = propertyName;
            activePaintable = dataObject.GetPaintable(activeGameObjectInfo);

            Load();
            Activate();

            Camera.onPostRender += PostRender;
        }

        public void AddBrushStroke(BrushStroke brushStroke)
        {
            brushStrokeQueue.Enqueue(brushStroke);
        }

        void Update()
        {
            //if (reset)
            //{
            //    SetGameObjectInfo(null as GameObjectInfo, null, true);
            //    reset = false;
            //}

            //if (switchTo)
            //{
            //    SetGameObjectInfo(switchTo, propertyName, true);
            //    switchTo = null;
            //}

            if (activeGameObjectInfo is null)
            {
                brushStrokeQueue.Clear();
                return;
            }

            DefaultUpdate();
        }

        void PostRender(Camera camera)
        {
            activePaintable?.PostRender();
        }

        void Load()
        {
            gameObjectInfos.RemoveAll(x => !x.GetGameObject());
            gameObjectInfos.ForEach(x => x.Load(this));
        }

        void Activate()
        {
#if UNITY_EDITOR
            activePaintable?.Activate(activeGameObjectInfo, activePropertyName);
#endif
        }

        void DefaultUpdate()
        {
            if (activePaintable is null)
                return;

            if (brushStrokeQueue.Count == 0)
            {
                activePaintable.Update(null);
                return;
            }

            while (brushStrokeQueue.Count > 0)
                activePaintable.Update(brushStrokeQueue.Dequeue());
        }

        void Deactivate()
        {
#if UNITY_EDITOR
            activePaintable?.Deactivate();
#endif
        }

        void Unload()
        {
            gameObjectInfos.ForEach(x => x.Unload());
        }

#if UNITY_EDITOR
        public bool CreateDataObject(GameObject gameObject)
        {
            if (this.dataObject)
                return false;

            var scene = gameObject.scene;
            var name = scene.name;
            var path = scene.path;
            var folderPath = path[..(path.Length - name.Length - 7)];

            var dataObjectPath = $"{folderPath}/{name}.asset";
            var dataObject = AssetDatabase.LoadAssetAtPath<TexturePainterDataObject>(dataObjectPath);

            if (!dataObject)
            {
                dataObject = ScriptableObject.CreateInstance<TexturePainterDataObject>();
                AssetDatabase.CreateAsset(dataObject, dataObjectPath);
                AssetDatabase.SaveAssets();
            }

            this.dataObject = dataObject;

            return true;
        }

        public bool CreateTexture(GameObject gameObject, string propertyName, int size, Color color)
        {
            var paintable = Register(gameObject);

            var textureInfo = paintable.GetTextureInfo(propertyName);
            if (textureInfo is not null)
                return false;

            SetGameObjectInfo(null, null);
            paintable.AddTexture(dataObject, gameObject.name, propertyName, size, color);
            return true;
        }
#endif
    }
}
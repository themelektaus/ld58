using UnityEngine;

using Temp = System.NonSerializedAttribute;

namespace TexturePainter
{
    using MaterialPropertyOverride = MaterialPropertyOverride.MaterialPropertyOverride;

    [System.Serializable]
    public class GameObjectInfo
    {
        public static GameObjectInfo Create(long id, GameObject gameObject)
            => new() { id = id, gameObject = gameObject };

        [SerializeField] long id;
        public long GetId() => id;

        [SerializeField] GameObject gameObject;
        public GameObject GetGameObject() => gameObject;
        public Matrix4x4 localToWorldMatrix => gameObject.transform.localToWorldMatrix;

        [Temp] TexturePainterDataObject dataObject;
        [Temp] MeshFilter meshFilter;
        [Temp] SkinnedMeshRenderer skinnedMeshRenderer;

        public Mesh mesh => meshFilter ? meshFilter.sharedMesh : skinnedMeshRenderer.sharedMesh;

        public Paintable GetPaintable()
            => dataObject.GetPaintable(this);

        public void Load(TexturePainterComponent component)
        {
            dataObject = component.dataObject;
            meshFilter = gameObject.GetComponent<MeshFilter>();
            skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        public void Unload()
        {
            dataObject = null;
            meshFilter = null;
            skinnedMeshRenderer = null;
        }

#if UNITY_EDITOR
        public void SetTexture(string name, Texture value)
        {
            var materialPropertyOverride = gameObject.GetComponent<MaterialPropertyOverride>();
            if (!materialPropertyOverride)
                materialPropertyOverride = gameObject.AddComponent<MaterialPropertyOverride>();

            materialPropertyOverride.SetTexture(name, value, new Vector4(1, 1, 0, 0));
        }

        public void UnsetTexture(string name)
        {
            if (gameObject.TryGetComponent<MaterialPropertyOverride>(out var materialPropertyOverride))
                materialPropertyOverride.UnsetTexture(name);
        }
#endif
    }
}

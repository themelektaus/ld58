using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Parallax")]
    public class Parallax : MonoBehaviour
    {
        public Transform optionalCamT;
        public UpdateMode updateMode = UpdateMode.LateUpdate;
        public List<ParallaxLayer> layers = new();

        [SerializeField, HideInInspector] int layersCount;

        Vector3 _camInitPos;

        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        [System.Serializable]
        public struct ParallaxLayer
        {
            public Transform transform;
            [HideInInspector]
            public Vector3 initPos;
            public float depth;
            [Range(0, 2)]
            public float horizontal;
            [Range(0, 2)]
            public float vertical;
            public Vector2 maxDistance;
        }

        void Awake()
        {
            if (!optionalCamT) optionalCamT = Utils.GetMainCamera(autoCreate: true).transform;

            _camInitPos = optionalCamT.position;

            for (int i = 0; i < layers.Count; i++)
            {
                ParallaxLayer layer = layers[i];
                layer.initPos = layer.transform.position;
                layers[i] = layer;
            }
        }

        void Update()
        {
            if (updateMode == UpdateMode.Update)
            {
                UpdateParralax();
            }
        }

        void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                UpdateParralax();
            }
        }

        void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
            {
                UpdateParralax();
            }
        }

        void UpdateParralax()
        {
            Vector3 moveVec = optionalCamT.position - _camInitPos;

            foreach (ParallaxLayer layer in layers)
            {
                Vector3 scale = new Vector3(layer.horizontal, layer.vertical);
                float depth = layer.depth;
                if (depth < Mathf.Epsilon) depth = 1;
                Vector3 offset = Vector3.Scale(moveVec, scale) * (1 / depth);
                if (layer.maxDistance.x > 0.01f)
                    offset.x = Mathf.Clamp(offset.x,
                        -layer.maxDistance.x, layer.maxDistance.x);
                if (layer.maxDistance.y > 0.01f)
                    offset.y = Mathf.Clamp(offset.y,
                        -layer.maxDistance.y, layer.maxDistance.y);
                layer.transform.position = layer.initPos + offset;
            }
        }

        void OnValidate()
        {
            if (layersCount != layers.Count)
            {
                while (layersCount < layers.Count)
                {
                    layersCount++;
                    layers[layersCount - 1] = new ParallaxLayer()
                    {
                        depth = -1,
                        horizontal = 1,
                        vertical = 1
                    };
                }

                layersCount = layers.Count;
            }
        }
    }
}

using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

using Temp = System.NonSerializedAttribute;

namespace TexturePainter
{
    [System.Serializable]
    public class PaintableTexture
    {
        public RenderTexture paintedRenderTexture { get; private set; }
        public RenderTexture runtimeRenderTexture { get; private set; }

        [Temp] RenderTexture fixedIslandsRenderTexture;
        [Temp] RenderTexture markedIslandsRenderTexture;

        [Temp] Material paintMaterial;
        [Temp] Material fixedEdgesMaterial;

        [Temp] GameObjectInfo gameObjectInfo;
        [Temp] TextureInfo textureInfo;

        CommandBuffer mainBuffer;
        CommandBuffer tempBuffer;

        [Temp] Camera camera;

        int frame;

#if UNITY_EDITOR
        public void Activate(Paintable paintable, GameObjectInfo gameObjectInfo, string propertyName)
        {
            if (frame > 0 && frame < 3)
                return;

            // reset
            Deactivate(false);

            // assert game object info
            if (gameObjectInfo is null)
                return;

            // Check texture info
            var textureInfo = paintable.textureInfos.FirstOrDefault(x => x.propertyName == propertyName);
            if (textureInfo is null)
                return;

            // setup references
            this.gameObjectInfo = gameObjectInfo;
            this.textureInfo = textureInfo;

            var texture = textureInfo?.texture;
            var width = texture.width;
            var height = texture.height;

            // create render texture for marked islands
            markedIslandsRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);

            // create painted render texture
            paintedRenderTexture = Utils.CreateRenderTexture(width, height);

            // create runtime render texture
            runtimeRenderTexture = Utils.CreateRenderTexture(width, height);

            // Set texture of the target
            gameObjectInfo.SetTexture(textureInfo.propertyName, runtimeRenderTexture);

            // create render texture for fixed islands
            fixedIslandsRenderTexture = new RenderTexture(paintedRenderTexture.descriptor);

            // create paint material
            paintMaterial = new(Utils.GetShader("Paint"));
            paintMaterial.SetPass(0);
            paintMaterial.SetTexture("_MainTex", paintedRenderTexture);

            // create material for fixed island edges
            fixedEdgesMaterial = new(Utils.GetShader("Fix Island Edges"));
            fixedEdgesMaterial.SetTexture("_IslandMap", markedIslandsRenderTexture);
            fixedEdgesMaterial.SetTexture("_MainTex", paintedRenderTexture);

            // create and add main command buffer
            mainBuffer = new() { name = "TexturePainting" };

            mainBuffer.SetRenderTarget(runtimeRenderTexture);
            mainBuffer.DrawMesh(gameObjectInfo.mesh, Matrix4x4.identity, paintMaterial);

            mainBuffer.Blit(runtimeRenderTexture, fixedIslandsRenderTexture, fixedEdgesMaterial);
            mainBuffer.Blit(fixedIslandsRenderTexture, runtimeRenderTexture);
            mainBuffer.Blit(runtimeRenderTexture, paintedRenderTexture);

            // create and add temp command buffer
            tempBuffer = new() { name = "MarkingIslands" };
            tempBuffer.SetRenderTarget(markedIslandsRenderTexture);

            var material = new Material(Utils.GetShader("Fix Island Marker"));
            tempBuffer.DrawMesh(gameObjectInfo.mesh, Matrix4x4.identity, material);

            UpdateCamera();
        }
#endif

        public void Update(BrushStroke? brushStroke)
        {
            UpdateCamera();

            if (!camera)
                return;

            paintMaterial.SetMatrix("mesh_Object2World", gameObjectInfo.localToWorldMatrix);

            bool draw = false;

            var _brushStroke = new BrushStroke
            {
                points = new() { null, null },
                brush = {
                    property = "_MainTex",
                    color = Color.white,
                    size = .1f,
                }
            };

            if (Application.isPlaying)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                    _brushStroke.points[0] = hit.point;

                var leftMouse = Input.GetMouseButton(0);
                var rightMouse = Input.GetMouseButton(1);

                draw = leftMouse || rightMouse;

                if (leftMouse)
                    _brushStroke.brush.color = Color.white;
            }
            else if (brushStroke.HasValue)
            {
                draw = true;
                _brushStroke = brushStroke.Value;
            }

            var points = new Vector4[2];

            for (int i = 0; i < _brushStroke.points.Count; i++)
            {
                if (!_brushStroke.points[i].HasValue)
                {
                    continue;
                }

                var point = _brushStroke.points[i].Value;

                points[i].x = point.x;
                points[i].y = point.y;
                points[i].z = point.z;
                points[i].w = draw ? 1 : 0;
            }

            paintMaterial.SetVectorArray("_Points", points);

            paintMaterial.SetColor("_BrushColor", _brushStroke.brush.color);
            paintMaterial.SetFloat("_BrushSize", _brushStroke.fill ? float.PositiveInfinity : _brushStroke.brush.size);
            paintMaterial.SetFloat("_BrushHardness", _brushStroke.brush.hardness);
        }

        public void PostRender()
        {
            if (!camera)
                return;

            frame++;

            if (frame == 2)
            {
                ClearRenderTextures();
                LoadTexture();
                return;
            }

            if (frame == 3)
            {
                RemoveCommandBuffer(tempBuffer);
                return;
            }
        }

#if UNITY_EDITOR
        public void Deactivate(bool save)
        {
            if (frame < 3)
                return;

            // unset references
            if (gameObjectInfo is not null)
            {
                // save texture
                var texture = save ? SaveTexture() : textureInfo?.texture;

                // assign texture to target
                gameObjectInfo.SetTexture(textureInfo.propertyName, texture);

                // unset target
                gameObjectInfo = null;
            }
            textureInfo = null;

            // Update camera
            UpdateCamera();

            // destroy materials
            Object.DestroyImmediate(paintMaterial);
            Object.DestroyImmediate(fixedEdgesMaterial);

            // unset materials
            paintMaterial = null;
            fixedEdgesMaterial = null;

            // Unset render target before destroying render textures
            Graphics.SetRenderTarget(null);

            // destroy render textures
            Object.DestroyImmediate(paintedRenderTexture);
            Object.DestroyImmediate(runtimeRenderTexture);
            Object.DestroyImmediate(fixedIslandsRenderTexture);
            Object.DestroyImmediate(markedIslandsRenderTexture);

            // unset render textures
            paintedRenderTexture = null;
            runtimeRenderTexture = null;
            fixedIslandsRenderTexture = null;
            markedIslandsRenderTexture = null;
        }
#endif

        void UpdateCamera()
        {
            // choose camera based on edit/play mode
            var camera = gameObjectInfo is null ? null : Application.isPlaying ? Camera.main : Camera.current;
            if (this.camera == camera)
                return;

            RemoveCommandBuffer(tempBuffer);
            RemoveCommandBuffer(mainBuffer);

            this.camera = camera;
            frame = 0;

            if (!camera)
                return;

            AddCommandBuffer(mainBuffer);
            AddCommandBuffer(tempBuffer);
        }

        void ClearRenderTextures()
        {
            // Clear runtime render texture
            Graphics.SetRenderTarget(runtimeRenderTexture);
            GL.Clear(false, true, Color.grey);

            // Clear painted render texture
            Graphics.SetRenderTarget(paintedRenderTexture);
            GL.Clear(false, true, Color.grey);
        }

        void LoadTexture()
        {
            var texture = textureInfo?.texture;
            if (texture)
                Graphics.Blit(texture, paintedRenderTexture);
        }

        Texture2D SaveTexture()
        {
            var texture = textureInfo?.texture;
#if UNITY_EDITOR
            if (texture)
                paintedRenderTexture.WriteTo(texture);
#endif
            return texture;
        }

        void AddCommandBuffer(CommandBuffer commandBuffer)
        {
            if (camera && commandBuffer is not null)
                camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, commandBuffer);
        }

        void RemoveCommandBuffer(CommandBuffer commandBuffer)
        {
            if (camera && commandBuffer is not null)
                camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, commandBuffer);
        }
    }
}
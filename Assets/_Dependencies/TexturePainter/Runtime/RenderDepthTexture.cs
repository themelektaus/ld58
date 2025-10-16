using UnityEngine;

namespace TexturePainter
{
	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	public class RenderDepthTexture : MonoBehaviour
	{
		Camera _camera;

		void Awake()
		{
			_camera = GetComponent<Camera>();
		}

		void OnEnable()
		{
			_camera.depthTextureMode = DepthTextureMode.Depth;
		}

		void OnDisable()
		{
			_camera.depthTextureMode = DepthTextureMode.None;
		}
	}
}
using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using Flags = System.Reflection.BindingFlags;
#endif

namespace TexturePainter
{
	public static partial class Utils
	{
		readonly static Dictionary<string, Shader> shaders = new();

		public static Shader GetShader(string name)
		{
			if (!shaders.ContainsKey(name))
				shaders.Add(name, Shader.Find($"Hidden/Texture Painter/{name}"));

			return shaders[name];
		}

#if UNITY_EDITOR
		static readonly Dictionary<System.Type, Object[]> assets = new();

		public static T LoadAsset<T>() where T : ScriptableObject
		{
			var asset = FindAssets<T>().FirstOrDefault();
			if (asset is null)
			{
				var type = typeof(T);

				var flags = Flags.Public | Flags.NonPublic | Flags.Static;

				var createMethod = type.GetMethod("Create", flags);
				if (createMethod is null)
					asset = ScriptableObject.CreateInstance<T>();
				else
					asset = createMethod.Invoke(null, null) as T;

				var name = asset.name;
				if (string.IsNullOrEmpty(name))
					name = type.Name;

				var path = $"Assets/{name}.asset";

				var fileInfo = new System.IO.FileInfo(path);
				fileInfo.Directory.Create();

				AssetDatabase.CreateAsset(asset, path);
				if (assets.ContainsKey(type))
					assets.Remove(type);
			}
			return asset;
		}

		public static IEnumerable<T> FindAssets<T>() where T : Object
		{
			var type = typeof(T);

			if (assets.ContainsKey(type))
				goto Return;

			var _assets = new List<T>();

			var assetGUIDs = AssetDatabase.FindAssets($"t:{type.Name}");
			foreach (var assetGUID in assetGUIDs)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if (!asset)
					continue;

				_assets.Add(asset);
			}

			assets[type] = _assets.ToArray();

		Return:
			return assets[type].Cast<T>();
		}

		public static void RefreshAssetDabase()
		{
			AssetDatabase.Refresh();
		}

		public static void SaveAssets()
		{
			AssetDatabase.SaveAssets();
		}

		public static IEnumerable<Object> EnumerateNestedAssets(Object parent)
		{
			var assetPath = AssetDatabase.GetAssetPath(parent);
			var nestedAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			return nestedAssets.Where(x => x != parent).OrderBy(x => x.name);
		}

		static readonly Dictionary<(int size, Color color), Texture2D> textureCache = new();

		public static Texture2D CreateTexture(string name, int size, Color color)
		{
			var cacheKey = (size, color);
            if (!textureCache.ContainsKey(cacheKey))
                textureCache.Add(cacheKey, CreateTexture(size, color));

			var texture = new Texture2D(size, size, TextureFormat.RGB24, false)
			{
				name = name,
				wrapMode = TextureWrapMode.Clamp
			};

			Graphics.CopyTexture(textureCache[cacheKey], texture);

			return texture;
		}

		static Texture2D CreateTexture(int size, Color color)
		{
			var texture = new Texture2D(size, size, TextureFormat.RGB24, false);

			for (int x = 0; x < texture.width; x++)
				for (int y = 0; y < texture.height; y++)
					texture.SetPixel(x, y, color);

			texture.Apply();

			return texture;
		}

		static Texture2D CreateTextureWithRandomColor(int size, Color min, Color max)
		{
			return CreateTexture(size, new(
				Mathf.Lerp(min.r, max.r, Random.value),
				Mathf.Lerp(min.g, max.g, Random.value),
				Mathf.Lerp(min.b, max.b, Random.value)
			));
		}

		public static RenderTexture CreateRenderTexture(int width, int height)
		{
			return new(width, height, 0)
			{
				anisoLevel = 0,
				useMipMap = false,
				filterMode = FilterMode.Bilinear
			};
		}

		static TexturePainterComponent texturePainterComponent;

		public static TexturePainterComponent GetTexturePainterComponent()
		{
			if (!texturePainterComponent)
			{
				texturePainterComponent = Object.FindAnyObjectByType<TexturePainterComponent>();
				if (!texturePainterComponent)
				{
					var gameObject = new GameObject("Texture Painter");
					texturePainterComponent = gameObject.AddComponent<TexturePainterComponent>();
				}
			}
			return texturePainterComponent;
		}
#endif
	}
}
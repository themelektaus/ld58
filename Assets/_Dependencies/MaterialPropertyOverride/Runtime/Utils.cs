using UnityEngine;

namespace MaterialPropertyOverride
{
	public static class Utils
	{
		static Texture2D _emptyTexture;

		public static Texture2D emptyTexture
			=> _emptyTexture = _emptyTexture
				? _emptyTexture
				: new(1, 1, TextureFormat.ARGB32, false);
	}
}
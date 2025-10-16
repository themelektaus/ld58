using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public class TextureScaleAndOffsetPropertyRenderer : VectorPropertyRenderer
	{
		public TextureScaleAndOffsetPropertyRenderer(MaterialPropertyOverrideEditor editor) : base(editor)
		{

		}

		protected override Vector4 GetOriginValue(string propertyName, Material material)
		{
			return base.GetOriginValue(propertyName + "_ST", material);
		}

		protected override void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, Vector4 value)
		{
			base.SetValue(materialPropertyOverride, name + "_ST", value);
		}

		protected override Vector4 RenderView(string labelName, Vector4 value)
		{
			return base.RenderView(labelName + " (Scale, Offset)", value);
		}

		protected override bool RenderOverrideView(string labelName, SerializedProperty valueProperty)
		{
			return base.RenderOverrideView(labelName + " (Scale, Offset)", valueProperty);
		}
	}
}
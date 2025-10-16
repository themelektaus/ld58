using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public class TexturePropertyRenderer : PropertyRenderer<Texture>
	{
		public TexturePropertyRenderer(MaterialPropertyOverrideEditor editor) : base(editor)
		{

		}

		protected override Texture GetOriginValue(string propertyName, Material material)
		{
			return material.GetTexture(propertyName);
		}

		protected override void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, Texture value)
		{
			renderer.ApplyPropertyBlock(x => x.SetTexture(name, value ? value : Utils.emptyTexture));
		}

		protected override void SetValue(SerializedProperty property, Texture value)
		{
			property.objectReferenceValue = value;
		}

		protected override Texture GetValue(SerializedProperty property)
		{
			return property.objectReferenceValue as Texture;
		}

		protected override Texture RenderView(string labelName, Texture value)
		{
			return EditorGUILayout.ObjectField(labelName + " (Texture)", value, typeof(Object), false) as Texture;
		}

		protected override bool RenderOverrideView(string labelName, SerializedProperty valueProperty)
		{
			return base.RenderOverrideView(labelName + " (Texture)", valueProperty);
		}
	}
}
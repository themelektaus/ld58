using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public class ColorPropertyRenderer : PropertyRenderer<Color>
	{
		public ColorPropertyRenderer(MaterialPropertyOverrideEditor editor) : base(editor)
		{

		}

		protected override Color GetOriginValue(string propertyName, Material material)
		{
			return material.GetColor(propertyName);
		}

		protected override void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, Color value)
		{
			renderer.ApplyPropertyBlock(x => x.SetColor(name, value));
		}

		protected override Color GetValue(SerializedProperty property)
		{
			return property.colorValue;
		}

		protected override void SetValue(SerializedProperty property, Color value)
		{
			property.colorValue = value;
		}

		protected override Color RenderView(string labelName, Color value)
		{
			return EditorGUILayout.ColorField(labelName, value);
		}
	}
}
using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public class FloatPropertyRenderer : PropertyRenderer<float>
	{
		public FloatPropertyRenderer(MaterialPropertyOverrideEditor editor) : base(editor)
		{

		}

		protected override float GetOriginValue(string propertyName, Material material)
		{
			return material.GetFloat(propertyName);
		}

		protected override void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, float value)
		{
			renderer.ApplyPropertyBlock(x => x.SetFloat(name, value));
		}

		protected override float GetValue(SerializedProperty property)
		{
			return property.floatValue;
		}

		protected override void SetValue(SerializedProperty property, float value)
		{
			property.floatValue = value;
		}

		protected override float RenderView(string labelName, float value)
		{
			return EditorGUILayout.FloatField(labelName, value);
		}
	}
}
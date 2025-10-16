using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public class VectorPropertyRenderer : PropertyRenderer<Vector4>
	{
		public VectorPropertyRenderer(MaterialPropertyOverrideEditor editor) : base(editor)
		{

		}

		protected override Vector4 GetOriginValue(string propertyName, Material material)
		{
			return material.GetVector(propertyName);
		}

		protected override void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, Vector4 value)
		{
			renderer.ApplyPropertyBlock(x => x.SetVector(name, value));
		}

		protected override Vector4 GetValue(SerializedProperty property)
		{
			return property.vector4Value;
		}

		protected override void SetValue(SerializedProperty property, Vector4 value)
		{
			property.vector4Value = value;
		}

		protected override Vector4 RenderView(string labelName, Vector4 value)
		{
			return EditorGUILayout.Vector4Field(labelName, value);
		}

		protected override bool RenderOverrideView(string labelName, SerializedProperty valueProperty)
		{
			var oldValue = valueProperty.vector4Value;
			var newValue = EditorGUILayout.Vector4Field(labelName, oldValue);

			if (oldValue == newValue)
				return false;

			valueProperty.vector4Value = newValue;
			return true;
		}
	}
}
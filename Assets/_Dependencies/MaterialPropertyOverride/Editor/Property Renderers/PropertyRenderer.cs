using UnityEditor;
using UnityEngine;

namespace MaterialPropertyOverride.Editor
{
	public abstract class PropertyRenderer<T>
	{
		readonly MaterialPropertyOverrideEditor editor;
		readonly MaterialPropertyOverride materialPropertyOverride;
		protected readonly Renderer renderer;

		protected PropertyRenderer(MaterialPropertyOverrideEditor editor)
		{
			this.editor = editor;
			materialPropertyOverride = this.editor.target as MaterialPropertyOverride;
			renderer = materialPropertyOverride.GetRenderer();
		}

		protected abstract T GetOriginValue(string propertyName, Material material);
		protected abstract void SetValue(MaterialPropertyOverride materialPropertyOverride, string name, T value);
		protected abstract T GetValue(SerializedProperty property);
		protected abstract void SetValue(SerializedProperty property, T value);
		protected abstract T RenderView(string labelName, T value);
		protected virtual bool RenderOverrideView(string labelName, SerializedProperty valueProperty)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(valueProperty, new GUIContent(labelName));
			return EditorGUI.EndChangeCheck();
		}

		SerializedProperty GetProperty(string propertiesName)
		{
			return editor.serializedObject.FindProperty(propertiesName);
		}

		static int GetPropertyNameIndex(SerializedProperty property, string propertyName)
		{
			return property.IndexOfArrayItem(x => x.FindPropertyRelative("name").stringValue == propertyName);
		}

		public void Render(string propertyName, string propertiesName, Material material)
		{
			Render(propertyName, GetRenderInfo(propertyName, propertiesName, material));
		}

		public bool IsEnabled(string propertyName, string propertiesName, Material material, out (T originValue, SerializedProperty property, int index) renderInfo)
		{
			renderInfo = GetRenderInfo(propertyName, propertiesName, material);
			return renderInfo.index >= 0;
		}

		public bool RenderIfEnabled(string propertyName, string propertiesName, Material material)
		{
			if (IsEnabled(propertyName, propertiesName, material, out var renderInfo))
			{
				Render(propertyName, renderInfo);
				return true;
			}
			return false;
		}

		public bool RenderIfDisabled(string propertyName, string propertiesName, Material material)
		{
			if (!IsEnabled(propertyName, propertiesName, material, out var renderInfo))
			{
				Render(propertyName, renderInfo);
				return true;
			}
			return false;
		}

		(T originValue, SerializedProperty property, int index) GetRenderInfo(string propertyName, string propertiesName, Material material)
		{
			var originValue = GetOriginValue(propertyName, material);
			var property = GetProperty(propertiesName);
			int index = GetPropertyNameIndex(property, propertyName);
			return (originValue, property, index);
		}

		public void Render(string propertyName, (T originValue, SerializedProperty property, int index) renderInfo)
		{
			if (renderInfo.index < 0)
			{
				var toggle = GUILayout.Toggle(false, "", GUILayout.Width(20));
				EditorGUI.BeginDisabledGroup(true);
				var value = RenderView(propertyName.TrimStart('_'), renderInfo.originValue);
				EditorGUI.EndDisabledGroup();
				if (toggle)
				{
					renderInfo.property.AddArrayItem(x =>
					{
						x.FindPropertyRelative("name").stringValue = propertyName;
						SetValue(x.FindPropertyRelative("value"), value);
					});
					SetValue(materialPropertyOverride, propertyName, value);
					editor.serializedObject.ApplyModifiedProperties();
				}
			}
			else
			{
				var toggle = GUILayout.Toggle(true, "", GUILayout.Width(20));
				var valueProperty = renderInfo.property.GetArrayElementAtIndex(renderInfo.index).FindPropertyRelative("value");
				if (RenderOverrideView(propertyName.TrimStart('_'), valueProperty))
				{
					var value = GetValue(valueProperty);
					SetValue(materialPropertyOverride, propertyName, value);
					editor.serializedObject.ApplyModifiedProperties();
				}
				else if (!toggle)
				{
					renderInfo.property.DeleteArrayElementAtIndex(renderInfo.index);
					SetValue(materialPropertyOverride, propertyName, renderInfo.originValue);
					editor.serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}
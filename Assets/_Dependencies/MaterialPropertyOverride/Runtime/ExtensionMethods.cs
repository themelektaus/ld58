using System;

using UnityEngine;

namespace MaterialPropertyOverride
{
	public static class ExtensionMethods
	{
#if UNITY_EDITOR
		public static void AddArrayItem(this UnityEditor.SerializedProperty property, Action<UnityEditor.SerializedProperty> action)
		{
			if (!property.isArray)
				throw new Exception("SerializedProperty must be array type");

			int index = property.arraySize;
			property.InsertArrayElementAtIndex(index);
			action?.Invoke(property.GetArrayElementAtIndex(index));
		}

		public static int IndexOfArrayItem(this UnityEditor.SerializedProperty property, Predicate<UnityEditor.SerializedProperty> predicate)
		{
			for (int i = 0; i < property.arraySize; i++)
				if (predicate(property.GetArrayElementAtIndex(i)))
					return i;

			return -1;
		}
#endif

		static MaterialPropertyBlock materialPropertyBlock;

		public static void ApplyPropertyBlock(this Renderer renderer, Action<MaterialPropertyBlock> action)
		{
			if (action is null)
				return;

			materialPropertyBlock ??= new();

			renderer.GetPropertyBlock(materialPropertyBlock);
			action(materialPropertyBlock);
			renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
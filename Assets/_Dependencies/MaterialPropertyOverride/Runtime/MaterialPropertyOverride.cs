using System;

using UnityEngine;

namespace MaterialPropertyOverride
{
    [ExecuteAlways]
    public class MaterialPropertyOverride : MonoBehaviour
    {
        public Renderer targetRenderer;

        [Serializable]
        public class ShaderProperty<T>
        {
            public string name;
            public T value;
        }

        [Serializable] public class ColorProperty : ShaderProperty<Color> { }
        [Serializable] public class FloatProperty : ShaderProperty<float> { }
        [Serializable] public class VectorProperty : ShaderProperty<Vector4> { }
        [Serializable] public class TextureProperty : ShaderProperty<Texture> { }

        public ColorProperty[] colorProperties = new ColorProperty[0];
        public FloatProperty[] floatProperties = new FloatProperty[0];
        public VectorProperty[] vectorProperties = new VectorProperty[0];
        public TextureProperty[] textureProperties = new TextureProperty[0];
        public VectorProperty[] textureScaleAndOffsetProperties = new VectorProperty[0];

#if UNITY_EDITOR
        bool refresh;
#endif

        public void Refresh()
        {
#if UNITY_EDITOR
            refresh = true;
#endif
        }

        public Renderer GetRenderer()
        {
            if (targetRenderer)
                return targetRenderer;

            if (TryGetComponent<Renderer>(out var renderer))
                return renderer;

            return null;
        }

        void OnEnable()
        {
            var renderer = GetRenderer();
            if (!renderer)
                return;

            OnDisable();

            renderer.ApplyPropertyBlock(x =>
            {
                if (renderer is SpriteRenderer spriteRenderer)
                {
                    spriteRenderer.GetPropertyBlock(x);
                    x.SetTexture("_MainTex", spriteRenderer.sprite.texture);
                    spriteRenderer.SetPropertyBlock(x);
                }

                SetupMaterialProperyBlockProperties(colorProperties, x.SetColor);
                SetupMaterialProperyBlockProperties(floatProperties, x.SetFloat);
                SetupMaterialProperyBlockProperties(vectorProperties, x.SetVector);
                SetupMaterialProperyBlockProperties(textureProperties, x.SetTexture);
                SetupMaterialProperyBlockProperties(textureScaleAndOffsetProperties, x.SetVector, "_ST");
            });
        }

        void OnDisable()
        {
            var renderer = GetRenderer();
            if (renderer)
                renderer.SetPropertyBlock(null);
        }

        void SetupMaterialProperyBlockProperties<T>(ShaderProperty<T>[] properties, Action<string, T> action, string suffix = null)
        {
            if (properties == null)
                return;

            foreach (var item in properties)
            {
                var name = item.name;
                if (suffix != null) name += suffix;
                action(name, item.value);
            }
        }

        void SetupMaterialProperyBlockProperties(TextureProperty[] properties, Action<string, Texture> action)
        {
            if (properties is null)
                return;

            foreach (var item in properties)
                action(item.name, item.value ? item.value : Utils.emptyTexture);
        }

#if UNITY_EDITOR
        void LateUpdate()
        {
            if (!refresh)
                return;

            refresh = false;
            OnDisable();
            OnEnable();
        }

        public void SetTexture(string name, Texture value, Vector4 scaleAndOffset)
        {
            if (!TryGetComponent<Renderer>(out var renderer))
                return;

            var serializedObject = new UnityEditor.SerializedObject(this);

            UnityEditor.SerializedProperty properties;
            int index;

            properties = serializedObject.FindProperty("textureProperties");
            index = properties.IndexOfArrayItem(x => x.FindPropertyRelative("name").stringValue == name);

            if (index == -1)
            {
                properties.AddArrayItem(property =>
                {
                    property.FindPropertyRelative("name").stringValue = name;
                    property.FindPropertyRelative("value").objectReferenceValue = value;
                });
            }
            else
            {
                var property = properties.GetArrayElementAtIndex(index);
                property.FindPropertyRelative("name").stringValue = name;
                property.FindPropertyRelative("value").objectReferenceValue = value;
            }

            var value_ST = new Vector4(1, 1, 0, 0);

            properties = serializedObject.FindProperty("textureScaleAndOffsetProperties");
            index = properties.IndexOfArrayItem(x => x.FindPropertyRelative("name").stringValue == name);

            if (index == -1)
            {
                properties.AddArrayItem(property =>
                {
                    property.FindPropertyRelative("name").stringValue = name;
                    property.FindPropertyRelative("value").vector4Value = value_ST;
                });
            }
            else
            {
                var property = properties.GetArrayElementAtIndex(index);
                property.FindPropertyRelative("name").stringValue = name;
                property.FindPropertyRelative("value").vector4Value = value_ST;
            }

            renderer.ApplyPropertyBlock(x =>
            {
                x.SetTexture(name, value ? value : Utils.emptyTexture);
                x.SetVector(name + "_ST", value_ST);
            });

            serializedObject.ApplyModifiedProperties();

            Refresh();
        }

        public void UnsetTexture(string name)
        {
            if (!TryGetComponent<Renderer>(out var renderer))
                return;

            var serializedObject = new UnityEditor.SerializedObject(this);

            UnityEditor.SerializedProperty properties;
            int index;

            properties = serializedObject.FindProperty("textureProperties");
            index = properties.IndexOfArrayItem(x => x.FindPropertyRelative("name").stringValue == name);

            if (index != -1)
                properties.DeleteArrayElementAtIndex(index);

            properties = serializedObject.FindProperty("textureScaleAndOffsetProperties");
            index = properties.IndexOfArrayItem(x => x.FindPropertyRelative("name").stringValue == name);

            if (index != -1)
                properties.DeleteArrayElementAtIndex(index);

            renderer.ApplyPropertyBlock(x =>
            {
                var value = renderer.sharedMaterial.GetTexture(name);
                if (value)
                    x.SetTexture(name, value);

                x.SetVector(name + "_ST", renderer.sharedMaterial.GetVector(name + "_ST"));
            });
            serializedObject.ApplyModifiedProperties();

            Refresh();
        }
#endif
    }
}

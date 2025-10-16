using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

#if UNITY_6000_2_OR_NEWER
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;
#else
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
#endif

namespace MaterialPropertyOverride.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaterialPropertyOverride))]
    public class MaterialPropertyOverrideEditor : UnityEditor.Editor
    {
        ColorPropertyRenderer colorPropertyRenderer;
        FloatPropertyRenderer floatPropertyRenderer;
        VectorPropertyRenderer vectorPropertyRenderer;
        TexturePropertyRenderer texturePropertyRenderer;
        TextureScaleAndOffsetPropertyRenderer textureScaleAndOffsetPropertyRenderer;

        Renderer _renderer;

        string search = "";
        bool showAll;

        void OnEnable()
        {
            _renderer = (target as MaterialPropertyOverride).GetRenderer();
            if (!_renderer)
                return;

            colorPropertyRenderer = new ColorPropertyRenderer(this);
            floatPropertyRenderer = new FloatPropertyRenderer(this);
            vectorPropertyRenderer = new VectorPropertyRenderer(this);
            texturePropertyRenderer = new TexturePropertyRenderer(this);
            textureScaleAndOffsetPropertyRenderer = new TextureScaleAndOffsetPropertyRenderer(this);
        }

        void OnDestroy()
        {
            SetMaterialEditable(_renderer, true);
        }

        void SetMaterialEditable(Renderer renderer, bool editable)
        {
            if (renderer)
                foreach (var material in renderer.sharedMaterials)
                    if (material)
                        material.hideFlags = editable ? HideFlags.None : HideFlags.NotEditable;
        }

        public override void OnInspectorGUI()
        {
            var materialPropertyOverride = target as MaterialPropertyOverride;
            var enabled = materialPropertyOverride.enabled;
            SetMaterialEditable(_renderer, !enabled);

            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(enabled);

                EditorGUIUtility.labelWidth = 50;
                materialPropertyOverride.targetRenderer = EditorGUILayout.ObjectField(
                    label: "Target",
                    obj: materialPropertyOverride.targetRenderer,
                    objType: typeof(Renderer),
                    allowSceneObjects: true,
                    GUILayout.ExpandWidth(true)
                ) as Renderer;

                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            var renderer = materialPropertyOverride.GetRenderer();

            if (!renderer)
                return;

            SetMaterialEditable(renderer, !enabled);

            var names = new HashSet<string>();

            EditorGUILayout.BeginHorizontal();
            {
                search = EditorGUILayout.TextField("Search", search, GUILayout.ExpandWidth(true));
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.Space(5, false);
                showAll = EditorGUILayout.Toggle("Show All", showAll, GUILayout.Width(80));
                EditorGUIUtility.labelWidth = labelWidth;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            OnInspectorGUI(names, materialPropertyOverride, renderer, true);
            OnInspectorGUI(names, materialPropertyOverride, renderer, false);
        }

        void OnInspectorGUI(HashSet<string> names, MaterialPropertyOverride materialPropertyOverride, Renderer renderer, bool enabled)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (!material)
                    continue;

                EditorGUI.BeginDisabledGroup(!materialPropertyOverride.enabled);

                var shader = material.shader;

#if UNITY_6000_2_OR_NEWER
                for (int i = 0; i < shader.GetPropertyCount(); i++)
#else
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
#endif
                {
#if UNITY_6000_2_OR_NEWER
                    string name = shader.GetPropertyName(i);
#else
                    string name = ShaderUtil.GetPropertyName(shader, i);
#endif
                    if (names.Contains(name))
                        continue;

                    if (!enabled && showAll && !name.ToLower().Contains(search.ToLower()))
                        continue;

                    if (!enabled && !showAll && (search == "" || !name.ToLower().Contains(search.ToLower())))
                        continue;

#if UNITY_6000_2_OR_NEWER
                    var type = shader.GetPropertyType(i);
#else
                    var type = ShaderUtil.GetPropertyType(shader, i);
#endif

                    EditorGUILayout.BeginHorizontal();

                    if (enabled)
                    {
                        switch (type)
                        {
                            case ShaderPropertyType.Color:
                                if (colorPropertyRenderer.RenderIfEnabled(name, "colorProperties", material))
                                    names.Add(name);
                                break;

                            case ShaderPropertyType.Float:
                            case ShaderPropertyType.Range:
                                if (floatPropertyRenderer.RenderIfEnabled(name, "floatProperties", material))
                                    names.Add(name);
                                break;

                            case ShaderPropertyType.Vector:
                                if (vectorPropertyRenderer.RenderIfEnabled(name, "vectorProperties", material))
                                    names.Add(name);
                                break;

#if UNITY_6000_2_OR_NEWER
                            case ShaderPropertyType.Texture:
#else
                            case ShaderPropertyType.TexEnv:
#endif
                                texturePropertyRenderer.RenderIfEnabled(name, "textureProperties", material);
                                if (textureScaleAndOffsetPropertyRenderer.IsEnabled(name, "textureScaleAndOffsetProperties", material, out var renderInfo))
                                {
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                    textureScaleAndOffsetPropertyRenderer.Render(name, renderInfo);
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case ShaderPropertyType.Color:
                                if (colorPropertyRenderer.RenderIfDisabled(name, "colorProperties", material))
                                    names.Add(name);
                                break;

                            case ShaderPropertyType.Float:
                            case ShaderPropertyType.Range:
                                if (floatPropertyRenderer.RenderIfDisabled(name, "floatProperties", material))
                                    names.Add(name);
                                break;

                            case ShaderPropertyType.Vector:
                                if (vectorPropertyRenderer.RenderIfDisabled(name, "vectorProperties", material))
                                    names.Add(name);
                                break;

#if UNITY_6000_2_OR_NEWER
                            case ShaderPropertyType.Texture:
#else
                            case ShaderPropertyType.TexEnv:
#endif
                                texturePropertyRenderer.RenderIfDisabled(name, "textureProperties", material);
                                if (!textureScaleAndOffsetPropertyRenderer.IsEnabled(name, "textureScaleAndOffsetProperties", material, out var renderInfo))
                                {
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                    textureScaleAndOffsetPropertyRenderer.Render(name, renderInfo);
                                }
                                break;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}

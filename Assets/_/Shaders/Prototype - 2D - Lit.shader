Shader "Prototype/2D/Lit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        
        [Toggle] _ADDITIVE_COLOR ("Additive Color", Float) = 0
        _AdditiveColor ("Additive Color", Vector) = (0,0,0,0)
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        [KeywordEnum(X0, X8, X12, X20, X28, X32)] _COLOR_REPLACEMENT ("Color Replacement", Float) = 0
        _ColorReplacementMap ("Color Replacement Map", 2D) = "black" {}
        [MaterialToggle] _DebugColorReplacement ("Debug Color Replacement", Float) = 0
        
        [KeywordEnum(Disabled, Enabled, Enabled with Normals)] _LIGHTING ("Lighting", Float) = 0
        [MaterialToggle] _LightingDisablePointLights ("Disable Point Lights", Float) = 0
        [MaterialToggle] _LightingDisableSpotLights ("Disable Spot Lights", Float) = 0
        _LightingStrength ("Lighting Strength", Range(0, 2)) = 0
        
        [HideInInspector] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 10)) = 1
        _NormalSmoothness ("Normal Smoothness", Range(0, 10)) = 1
        _NormalBrightness ("Normal Brightness", Range(0, 1)) = 1
        
        _OcclusionMap ("Occlusion Map", 2D) = "bump" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1
        _OcclusionBalance ("Occlusion Balance", Range(0, 2)) = 1
        _OcclusionPower ("Occlusion Power", Range(0, 10)) = 1
        
        [Toggle] _BLUR ("Blur", Float) = 0
        _BlurRadius ("Blur Radius", Range(0, 30)) = 4
        _BlurWeight ("Blur Weight", Range(0, 1)) = 1
        _BlurResolution ("Blur Resolution", float) = 800
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            
            #pragma multi_compile _ _ADDITIVE_COLOR_ON
            #pragma multi_compile _COLOR_REPLACEMENT_X0 _COLOR_REPLACEMENT_X8 _COLOR_REPLACEMENT_X12 _COLOR_REPLACEMENT_X20 _COLOR_REPLACEMENT_X28 _COLOR_REPLACEMENT_X32
            #pragma multi_compile _LIGHTING_DISABLED _LIGHTING_ENABLED _LIGHTING_ENABLED_WITH_NORMALS
            #pragma multi_compile _ _BLUR_ON
            
            #include "Prototype - 2D.cginc"
            
            #ifndef _COLOR_REPLACEMENT_X0
                sampler2D _ColorReplacementMap;
                int _DebugColorReplacement;
            #endif
            
            #ifdef _ADDITIVE_COLOR_ON
                fixed4 _AdditiveColor;
            #endif
            
            #ifndef _LIGHTING_DISABLED
                #include "Prototype - 2D - Lighting.cginc"
            #endif
            
            #ifndef _COLOR_REPLACEMENT_X0
                fixed4 ApplyColorReplacement(in v2f IN, fixed4 pixel, in int count)
                {
                    if (pixel.a)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            float u = (i + .5) / count;
                            fixed4 map = tex2D(_ColorReplacementMap, float2(u, 1));
                            
                            if (map.a == 0)
                                break;
                            
                            if (length(map.rgb - pixel.rgb))
                                continue;
                            
                            if (_DebugColorReplacement)
                                pixel = fixed4(0, 1, 1, 1);
                            else
                                pixel.rgb = tex2D(_ColorReplacementMap, float2(u, 0));
                            break;
                        }
                    }
                    
                    return pixel;
                }
            #endif
            
            #ifdef _BLUR_ON
                #include "Prototype - 2D - Blur.cginc"
            #endif
            
            fixed4 SpriteFrag(v2f IN) : SV_Target
            {
                #ifdef _BLUR_ON
                    fixed4 pixel = SampleSpriteTextureWithColorAndBlur(IN);
                #else
                    fixed4 pixel = SampleSpriteTextureWithColor(IN);
                #endif
                
                #ifdef _COLOR_REPLACEMENT_X8
                    pixel = ApplyColorReplacement(IN, pixel, 8);
                #endif
                #ifdef _COLOR_REPLACEMENT_X12
                    pixel = ApplyColorReplacement(IN, pixel, 12);
                #endif
                #ifdef _COLOR_REPLACEMENT_X20
                    pixel = ApplyColorReplacement(IN, pixel, 20);
                #endif
                #ifdef _COLOR_REPLACEMENT_X28
                    pixel = ApplyColorReplacement(IN, pixel, 28);
                #endif
                #ifdef _COLOR_REPLACEMENT_X32
                    pixel = ApplyColorReplacement(IN, pixel, 32);
                #endif
                #ifndef _LIGHTING_DISABLED
                    pixel = ApplyLighting(pixel, IN.texcoord, IN.worldpos);
                #endif
                #ifdef _ADDITIVE_COLOR_ON
                    pixel.rgb += _AdditiveColor.rgb * _AdditiveColor.a;
                #endif
                return ApplyAlpha(pixel);
            }
            
            ENDCG
        }
    }
}

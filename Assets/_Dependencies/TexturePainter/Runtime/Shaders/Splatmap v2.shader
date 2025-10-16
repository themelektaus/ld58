Shader "Texture Painter/Splatmap v2"
{
    Properties
    {
        [Header(Features)][Space]
        [Toggle] _COLOR ("Color", Float) = 0
        [Toggle] _ADVANCED_COLORS ("Advanced Colors", Float) = 0
        [Toggle] _NORMAL ("Normal", Float) = 0
        [Toggle] _ROUGHNESS ("Roughness", Float) = 0
        [Toggle] _AMBIENT_OCCLUSION ("Ambient Occlusion", Float) = 0
        [Toggle] _DISPLACEMENT ("Displacement", Float) = 0
        
        [Header(Splatmap)][Space]
        [NoScaleOffset] _Splatmap ("Texture", 2D) = "black" {}
        _SplatMapIndex ("Index", Vector) = (0, 1, 2, 3)
        
        [Header(Additive Color)][Space]
        [NoScaleOffset] _AdditiveColor ("Texture", 2D) = "black" {}
        _AdditiveColorStrength ("Strength", Float) = 1

        [Header(Subtractive Color)][Space]
        [NoScaleOffset] _SubtractiveColor ("Texture", 2D) = "white" {}
        _SubtractiveColorStrength ("Strength", Float) = 1
        
        [Header(Map)][Space]
        _MapArray_Scale ("Scale", Vector) = (1, 1, 1, 1)
        
        [Header(Map ... Color)][Space]
        [NoScaleOffset] _MapArray_Color ("Texture Array", 2DArray) = "white" {}
        
        [Header(Map ... Normal)][Space]
        [NoScaleOffset] _MapArray_Normal ("Texture Array", 2DArray) = "bump" {}
        _MapArray_NormalStrength ("Strength", Vector) = (1, 1, 1, 1)
        
        [Header(Map ... Roughness)][Space]
        [NoScaleOffset] _MapArray_Roughness ("Texture Array", 2DArray) = "gray" {}
        
        [Header(Map ... Ambient Occlusion)][Space]
        [NoScaleOffset] _MapArray_AmbientOcclusion ("Texture Array", 2DArray) = "white" {}
        
        [Header(Map ... Displacement)][Space]
        [NoScaleOffset] _MapArray_Displacement ("Texture Array", 2DArray) = "gray" {}
        _MapArray_DisplacementStrength ("Strength", Vector) = (1, 1, 1, 1)
        _MapArray_DisplacementOffset ("Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        
        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows vertex:vert
        
        #pragma multi_compile __ _COLOR_ON
        #pragma multi_compile __ _ADVANCED_COLORS_ON
        #pragma multi_compile __ _NORMAL_ON
        #pragma multi_compile __ _ROUGHNESS_ON
        #pragma multi_compile __ _AMBIENT_OCCLUSION_ON
        #pragma multi_compile __ _DISPLACEMENT_ON
        
        #pragma target 3.5
        
        sampler2D _Splatmap;
        float4 _SplatMapIndex;
        
        float4 _MapArray_Scale;
        
        #ifdef _COLOR_ON
            UNITY_DECLARE_TEX2DARRAY(_MapArray_Color);
        #endif
        
        #ifdef _ADVANCED_COLORS_ON
            sampler2D _AdditiveColor;
            fixed _AdditiveColorStrength;
            sampler2D _SubtractiveColor;
            fixed _SubtractiveColorStrength;
        #endif
        
        #ifdef _NORMAL_ON
            UNITY_DECLARE_TEX2DARRAY(_MapArray_Normal);
            float4 _MapArray_NormalStrength;
        #endif
        
        #ifdef _ROUGHNESS_ON
            UNITY_DECLARE_TEX2DARRAY(_MapArray_Roughness);
        #endif
        
        #ifdef _AMBIENT_OCCLUSION_ON
            UNITY_DECLARE_TEX2DARRAY(_MapArray_AmbientOcclusion);
        #endif
        
        #ifdef _DISPLACEMENT_ON
            UNITY_DECLARE_TEX2DARRAY(_MapArray_Displacement);
            float4 _MapArray_DisplacementStrength;
            float4 _MapArray_DisplacementOffset;
        #endif
        
        int getSplatmapIndex(int index)
        {
            return _SplatMapIndex[index];
        }
        
        fixed4 unpackSplatmap(fixed3 splatmap)
        {
            return fixed4(
                max(0, 1 - splatmap.r - splatmap.g - splatmap.b),
                splatmap.r,
                splatmap.g,
                splatmap.b
            );
        }
        
        void vert(inout appdata_full data)
        {
            #ifdef _DISPLACEMENT_ON
                float4 uv = float4(data.texcoord.xy, 0, 0);
                
                fixed4 splatmap = unpackSplatmap(tex2Dlod(_Splatmap, uv).rgb);
                
                fixed displacement;
                
                for (int i = 0; i < 4; i++)
                {
                    float height = UNITY_SAMPLE_TEX2DARRAY_LOD(
                        _MapArray_Displacement,
                        float3(uv.xy, i),
                        0
                    );
                    
                    displacement +=
                        (height - _MapArray_DisplacementOffset[i])
                            * splatmap[getSplatmapIndex(i)]
                            * _MapArray_DisplacementStrength[i];
                }
                
                data.vertex.xyz += data.normal * displacement;
            #endif
        }
        
        struct Input
        {
            float2 uv_Splatmap;
            float2 uv_MapArray_Color;
        };
        
        void surf(Input IN, inout SurfaceOutputStandard output)
        {
            fixed4 splatmap = unpackSplatmap(tex2D(_Splatmap, IN.uv_Splatmap));
            
            #ifdef _COLOR_ON
                fixed3 albedo;
            #else
                fixed3 albedo = 1/2.2;
            #endif
            
            #ifdef _NORMAL_ON
                half3 normal;
            #endif
            
            #ifdef _ROUGHNESS_ON
                fixed smoothness;
            #endif
            
            #if defined(_ADVANCED_COLORS_ON) || defined(_AMBIENT_OCCLUSION_ON)
                fixed occlusion = 1;
            #endif
            
            #if defined(_COLOR_ON) || defined(_NORMAL_ON) || defined(_ROUGHNESS_ON) || defined(_AMBIENT_OCCLUSION_ON)
                for (int i = 0; i < 4; i++)
                {
                    fixed s = splatmap[getSplatmapIndex(i)];
                    float3 index = float3(IN.uv_MapArray_Color * _MapArray_Scale[i], i);
                    
                    #ifdef _COLOR_ON
                        albedo += UNITY_SAMPLE_TEX2DARRAY(_MapArray_Color, index).rgb * s;
                    #endif
                    
                    #ifdef _NORMAL_ON
                        normal += UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_MapArray_Normal, index), _MapArray_NormalStrength[i]) * s;
                    #endif
                    
                    #ifdef _ROUGHNESS_ON
                        smoothness += UNITY_SAMPLE_TEX2DARRAY(_MapArray_Roughness, index).rgb * s;
                    #endif
                    
                    #ifdef _AMBIENT_OCCLUSION_ON
                        occlusion += UNITY_SAMPLE_TEX2DARRAY(_MapArray_AmbientOcclusion, index).rgb * s;
                    #endif
                }
            #endif
            
            #ifdef _ADVANCED_COLORS_ON
                fixed additiveColor = tex2D(_AdditiveColor, IN.uv_Splatmap).r * _AdditiveColorStrength;
                albedo.rgb *= pow(1 + additiveColor, 4);
                
                fixed subtractiveColor = min(1, (1 - tex2D(_SubtractiveColor, IN.uv_Splatmap).r) * _SubtractiveColorStrength);
                albedo.rgb *= min(pow((1 - subtractiveColor) + .5, 2), 1);
            #endif
            
            output.Albedo = albedo.rgb;
            
            #ifdef _NORMAL_ON
                output.Normal = normal;
            #endif
            
            #ifdef _ROUGHNESS_ON
                output.Smoothness = smoothness;
            #endif
            
            #ifdef _ADVANCED_COLORS_ON
                occlusion -= subtractiveColor;
            #endif
            
            #if defined(_ADVANCED_COLORS_ON) || defined(_AMBIENT_OCCLUSION_ON)
                output.Occlusion = occlusion;
            #endif
            
            output.Alpha = 1;
        }
        
        ENDCG
    }
    
    FallBack "Diffuse"
}

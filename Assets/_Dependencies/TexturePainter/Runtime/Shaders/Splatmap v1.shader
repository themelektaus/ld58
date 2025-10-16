Shader "Texture Painter/Splatmap v1"
{
    Properties
    {
        [NoScaleOffset] _Splatmap ("Splatmap", 2D) = "black" {}
        _ScaleOffset ("Scale / Offset", Vector) = (1, 1, 0, 0)
        
        [Header(Map 1)][Space][Space]
                        _Map1_Color                  ("Color", 2D)                 = "white" {}
        [NoScaleOffset] _Map1_Normal                 ("Normal", 2D)                = "bump"  {}
                        _Map1_NormalStrength         ("Strength", Float)           =  1
        [NoScaleOffset] _Map1_Roughness              ("Roughness", 2D)             = "gray"  {}
        [NoScaleOffset] _Map1_AmbientOcclusion       ("Ambient Occlusion", 2D)     = "white" {}
        [NoScaleOffset] _Map1_Displacement           ("Displacement", 2D)          = "gray"  {}
                        _Map1_DisplacementParameters ("Strength / Offset", Vector) = (1, 0, 0, 0)
        [Space][Space][Space]
        
        [Header(Map 2)][Space][Space]
                        _Map2_Color                  ("Color", 2D)                 = "white" {}
        [NoScaleOffset] _Map2_Normal                 ("Normal", 2D)                = "bump"  {}
                        _Map2_NormalStrength         ("Strength", Float)           =  1
        [NoScaleOffset] _Map2_Roughness              ("Roughness", 2D)             = "gray"  {}
        [NoScaleOffset] _Map2_AmbientOcclusion       ("Ambient Occlusion", 2D)     = "white" {}
        [NoScaleOffset] _Map2_Displacement           ("Displacement", 2D)          = "gray"  {}
                        _Map2_DisplacementParameters ("Strength / Offset", Vector) = (1, 0, 0, 0)
        [Space][Space][Space]
        
        [Header(Map 3)][Space][Space]
                        _Map3_Color                  ("Color", 2D)                 = "white" {}
        [NoScaleOffset] _Map3_Normal                 ("Normal", 2D)                = "bump"  {}
                        _Map3_NormalStrength         ("Strength", Float)           =  1
        [NoScaleOffset] _Map3_Roughness              ("Roughness", 2D)             = "gray"  {}
        [NoScaleOffset] _Map3_AmbientOcclusion       ("Ambient Occlusion", 2D)     = "white" {}
        [NoScaleOffset] _Map3_Displacement           ("Displacement", 2D)          = "gray"  {}
                        _Map3_DisplacementParameters ("Strength / Offset", Vector) = (1, 0, 0, 0)
        
        [Header(Map 4)][Space][Space]
                        _Map4_Color                  ("Color", 2D)                 = "white" {}
        [NoScaleOffset] _Map4_Normal                 ("Normal", 2D)                = "bump"  {}
                        _Map4_NormalStrength         ("Strength", Float)           =  1
        [NoScaleOffset] _Map4_Roughness              ("Roughness", 2D)             = "gray"  {}
        [NoScaleOffset] _Map4_AmbientOcclusion       ("Ambient Occlusion", 2D)     = "white" {}
        [NoScaleOffset] _Map4_Displacement           ("Displacement", 2D)          = "gray"  {}
                        _Map4_DisplacementParameters ("Strength / Offset", Vector) = (1, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        
        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5
        
        struct Input
        {
            float2 uv_Splatmap;
            float2 uv_Map1_Color;
            float2 uv_Map2_Color;
            float2 uv_Map3_Color;
            //float2 uv_Map4_Color;
        };
        
        sampler2D _Splatmap;
        float4 _ScaleOffset;
        
        sampler2D _Map1_Color;
        sampler2D _Map1_Normal;
            float _Map1_NormalStrength;
        sampler2D _Map1_Roughness;
        sampler2D _Map1_AmbientOcclusion;
        sampler2D _Map1_Displacement;
           float4 _Map1_DisplacementParameters;
        
        sampler2D _Map2_Color;
        sampler2D _Map2_Normal;
            float _Map2_NormalStrength;
        sampler2D _Map2_Roughness;
        sampler2D _Map2_AmbientOcclusion;
        sampler2D _Map2_Displacement;
           float4 _Map2_DisplacementParameters;
        
        sampler2D _Map3_Color;
        sampler2D _Map3_Normal;
            float _Map3_NormalStrength;
        sampler2D _Map3_Roughness;
        sampler2D _Map3_AmbientOcclusion;
        sampler2D _Map3_Displacement;
           float4 _Map3_DisplacementParameters;
        
        //sampler2D _Map4_Color;
        //sampler2D _Map4_Normal;
        //    float _Map4_NormalStrength;
        //sampler2D _Map4_Roughness;
        //sampler2D _Map4_AmbientOcclusion;
        //sampler2D _Map4_Displacement;
        //   float4 _Map4_DisplacementParameters;
        
        float2 getUV(float2 uv)
        {
            return uv * _ScaleOffset.xy + _ScaleOffset.zw;
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
            float4 uv = float4(getUV(data.texcoord.xy), 0, 0);
            
            fixed4 splatmap = unpackSplatmap(tex2Dlod(_Splatmap, uv).rgb);
            
            fixed map1_displacement = tex2Dlod(_Map1_Displacement, uv);
            fixed map2_displacement = tex2Dlod(_Map2_Displacement, uv);
            fixed map3_displacement = tex2Dlod(_Map3_Displacement, uv);
            //fixed map4_displacement = tex2Dlod(_Map4_Displacement, uv);
            
            fixed displacement =
                (map1_displacement - _Map1_DisplacementParameters.y) * splatmap.x * _Map1_DisplacementParameters.x +
                (map2_displacement - _Map2_DisplacementParameters.y) * splatmap.y * _Map2_DisplacementParameters.x +
                (map3_displacement - _Map3_DisplacementParameters.y) * splatmap.z * _Map3_DisplacementParameters.x;
                //(map4_displacement - _Map4_DisplacementParameters.y) * splatmap.w * _Map4_DisplacementParameters.x;
            
            data.vertex.xyz += data.normal * displacement;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard output)
        {
            fixed4 splatmap = unpackSplatmap(tex2D(_Splatmap, getUV(IN.uv_Splatmap.xy)));
            
            fixed3 map1_albedo = tex2D(_Map1_Color, getUV(IN.uv_Map1_Color)).rgb;
            fixed3 map2_albedo = tex2D(_Map2_Color, getUV(IN.uv_Map2_Color)).rgb;
            fixed3 map3_albedo = tex2D(_Map3_Color, getUV(IN.uv_Map3_Color)).rgb;
            //fixed3 map4_albedo = tex2D(_Map4_Color, getUV(IN.uv_Map4_Color)).rgb;
            fixed3 albedo =
                map1_albedo * splatmap.x +
                map2_albedo * splatmap.y +
                map3_albedo * splatmap.z;
                //map4_albedo * splatmap.w;
            
            half3 map1_normal = UnpackScaleNormal(tex2D(_Map1_Normal, getUV(IN.uv_Map1_Color)), _Map1_NormalStrength);
            half3 map2_normal = UnpackScaleNormal(tex2D(_Map2_Normal, getUV(IN.uv_Map2_Color)), _Map2_NormalStrength);
            half3 map3_normal = UnpackScaleNormal(tex2D(_Map3_Normal, getUV(IN.uv_Map3_Color)), _Map3_NormalStrength);
            //half3 map4_normal = UnpackScaleNormal(tex2D(_Map4_Normal, getUV(IN.uv_Map4_Color)), _Map4_NormalStrength);
            half3 normal =
                map1_normal * splatmap.x +
                map2_normal * splatmap.y +
                map3_normal * splatmap.z;
                //map4_normal * splatmap.w;
            
            fixed map1_smoothness = tex2D(_Map1_Roughness, getUV(IN.uv_Map1_Color));
            fixed map2_smoothness = tex2D(_Map2_Roughness, getUV(IN.uv_Map2_Color));
            fixed map3_smoothness = tex2D(_Map3_Roughness, getUV(IN.uv_Map3_Color));
            //fixed map4_smoothness = tex2D(_Map4_Roughness, getUV(IN.uv_Map4_Color));
            fixed3 smoothness =
                map1_smoothness * splatmap.x +
                map2_smoothness * splatmap.y +
                map3_smoothness * splatmap.z;
                //map4_smoothness * splatmap.w;
            
            fixed map1_occlusion = tex2D(_Map1_AmbientOcclusion, getUV(IN.uv_Map1_Color));
            fixed map2_occlusion = tex2D(_Map2_AmbientOcclusion, getUV(IN.uv_Map2_Color));
            fixed map3_occlusion = tex2D(_Map3_AmbientOcclusion, getUV(IN.uv_Map3_Color));
            //fixed map4_occlusion = tex2D(_Map4_AmbientOcclusion, getUV(IN.uv_Map4_Color));
            fixed3 occlusion =
                map1_occlusion * splatmap.x +
                map2_occlusion * splatmap.y +
                map3_occlusion * splatmap.z;
                //map4_occlusion * splatmap.w;
            
            output.Albedo = albedo;
            output.Normal = normal;
            output.Smoothness = smoothness;
            output.Occlusion = occlusion;
            output.Alpha = 1;
        }
        
        ENDCG
    }
    
    FallBack "Diffuse"
}

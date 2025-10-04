// Base shader found at https://stackoverflow.com/questions/29030321/unity3d-blur-the-background-of-a-ui-canvas

Shader "Prototype/Xplosive Billard/UI/Blur"
{
    Properties
    {
        [HideInInspector] _MainTex("Masking Texture", 2D) = "white" { }
        [HideInInspector] _Scale("Scale", Float) = 1

        _Weight("Weight", Range(0, 1)) = 1
        _Size("Blur", Range(0, 10)) = 3
        _MultiplyColor("Multiply Tint color", Color) = (1, 1, 1, 1)
        _AdditiveColor("Additive Tint color", Color) = (0, 0, 0, 0)
        _Brightness("Brightness", Range(-1, 1)) = 0
        _Contrast("Contrast", Range(-1, 1)) = 0
        _Saturation("Saturation", Range(0, 1)) = 1
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Opaque"
        }

        SubShader
        {
            GrabPass { "_HBlur" }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                
                #pragma vertex vert
                #pragma fragment frag
                
                sampler2D _HBlur;
                float4 _HBlur_TexelSize;

                #include "XB - UI - Blur 1.cginc"
                
                float4 grabPixel(float4 uvgrab, float weight, float a)
                {
                    uvgrab.x += _HBlur_TexelSize.x * getTexelSizeFactor(a);
                    return tex2Dproj(_HBlur, UNITY_PROJ_COORD(uvgrab)) * weight;
                }

                #include "XB - UI - Blur 2.cginc"

                half4 frag(v2f i) : COLOR
                {
                    half3 c = preFrag(i);
                    
                    return postFrag(i, c);
                }

                ENDCG
            }

            GrabPass { "_VBlur" }

            Pass
            {
                CGPROGRAM
                
                #pragma vertex vert
                #pragma fragment frag
                
                sampler2D _VBlur;
                float4 _VBlur_TexelSize;
                
                #include "XB - UI - Blur 1.cginc"
                
                float4 grabPixel(float4 uvgrab, float weight, float a)
                {
                    uvgrab.y += _VBlur_TexelSize.y * getTexelSizeFactor(a);
                    return tex2Dproj(_VBlur, UNITY_PROJ_COORD(uvgrab)) * weight;
                }
                
                #include "XB - UI - Blur 2.cginc"

                float4 _MultiplyColor;
                float4 _AdditiveColor;
                half _Brightness;
                half _Contrast;
                half _Saturation;

                half4 frag(v2f i) : COLOR
                {
                    half3 c = (preFrag(i) + _Brightness) * (_Contrast + 1);
                    
                    c = lerp(c, c * _MultiplyColor.rgb, _MultiplyColor.a * _Weight);
                    c = lerp(c, c + _AdditiveColor.rgb, _AdditiveColor.a * _Weight);
                    c = lerp(dot(c, float3(.299, .587, .114)), c, lerp(1, _Saturation, _Weight));

                    return postFrag(i, c);
                }
                
                ENDCG
            }
        }
    }
}

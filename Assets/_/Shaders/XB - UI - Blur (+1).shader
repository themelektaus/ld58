// Base shader found at https://stackoverflow.com/questions/29030321/unity3d-blur-the-background-of-a-ui-canvas

Shader "Prototype/Xplosive Billard/UI/Blur (+1)"
{
    Properties
    {
        [HideInInspector] _MainTex("Masking Texture", 2D) = "white" { }
        [HideInInspector] _Scale("Scale", Float) = 1

        _Weight("Weight", Range(0, 1)) = 1
        _Size("Blur", Range(0, 10)) = 3
        _MultiplyColor("Multiply Tint color", Color) = (1, 1, 1, 1)
        _AdditiveColor("Additive Tint color", Color) = (0, 0, 0, 0)
        _Saturation("Saturation", Range(0, 1)) = 1
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent+1"
            "IgnoreProjector" = "True"
            "RenderType" = "Opaque"
        }

        SubShader
        {
            GrabPass { "_HBlur2" }

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
                
                sampler2D _HBlur2;
                float4 _HBlur2_TexelSize;

                #include "XB - UI - Blur 1.cginc"
                
                float4 grabPixel(float4 uvgrab, float weight, float a)
                {
                    uvgrab.x += _HBlur2_TexelSize.x * getTexelSizeFactor(a);
                    return tex2Dproj(_HBlur2, UNITY_PROJ_COORD(uvgrab)) * weight;
                }

                #include "XB - UI - Blur 2.cginc"

                half4 frag(v2f i) : COLOR
                {
                    half3 c = preFrag(i);
                    
                    return postFrag(i, c);
                }

                ENDCG
            }

            GrabPass { "_VBlur2" }

            Pass
            {
                CGPROGRAM
                
                #pragma vertex vert
                #pragma fragment frag
                
                sampler2D _VBlur2;
                float4 _VBlur2_TexelSize;
                
                #include "XB - UI - Blur 1.cginc"
                
                float4 grabPixel(float4 uvgrab, float weight, float a)
                {
                    uvgrab.y += _VBlur2_TexelSize.y * getTexelSizeFactor(a);
                    return tex2Dproj(_VBlur2, UNITY_PROJ_COORD(uvgrab)) * weight;
                }
                
                #include "XB - UI - Blur 2.cginc"

                float4 _MultiplyColor;
                float4 _AdditiveColor;
                half _Saturation;

                half4 frag(v2f i) : COLOR
                {
                    half3 c = preFrag(i);
                    
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

Shader "Prototype/Unlit/Neon Circles"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _PPU("PPU", Int) = 0
        _PPU_Center("PPU Center", Range(0, 2)) = 1
        _Alpha1("Alpha1", Range(0, 1)) = 1
        _Alpha2("Alpha2", Range(0, 1)) = 1
        _Color("Color", Color) = (1, 1, 1, 1)
        _Zoom("Zoom", Float) = 6
        _Scale("Scale", Float) = .5
        _Exposure("Exposure", Float) = 2
        _Speed("Speed", Float) = .75
        _Iterations("Iterations", Int) = 6
        _Saturation("Saturation", Float) = .5
        _Power("Power", Float) = 1.75
        _ColorInterval("ColorInterval", Float) = .4
        _Shift("Shift", Float) = 1.5
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            sampler2D _MainTex;
            
            int _PPU;
            int _PPU_Center;
            float _Alpha1;
            float _Alpha2;
            fixed4 _Color;
            float _Zoom;
            float _Scale;
            float _Exposure;
            float _Speed;
            int _Iterations;
            float _Saturation;
            float _Power;
            float _ColorInterval;
            float _Shift;
            
            fixed4 frag(v2f i) : SV_Target
            {
                half PI = 3.14159;
                half4 c;
                float2 u = (i.uv * 2 - 1) * _Scale;
                
                if (_PPU > 0)
                {
                    float ppu = _PPU * (_PPU_Center > 0 ? .5 : 1);
                    
                    fixed2 s = fixed2(
                        length(unity_ObjectToWorld._m00_m10_m20),
                        length(unity_ObjectToWorld._m01_m11_m21)
                    ) / 2 * ppu / _Scale;
                    
                    u = lerp(round(u * s), floor(u * s) + .5, _PPU_Center / 3.0) / s;
                }
                
                float l = length(u);
                float t = _Time.y * _Speed;
                
                for (int i = 0; i <= _Iterations; i++)
                {
                    u = frac(u * _Shift) - .5;
                    
                    c += pow(
                        abs(
                            _Exposure * .01 / sin(
                                length(u) * exp(-l) * _Zoom + t
                            )
                        ), _Power
                    ) * (
                        .5 + .5 * cos(
                            _Saturation * PI * 4 * (
                                l + (i + t) * _ColorInterval + half4(.26, .42, .56, 0)
                            )
                        )
                    );
                }
                
                c = clamp(c, 0, 1);
                c.rgb = lerp(c.rgb, half3(1, 1, 1) - c.rgb, 1 - _Alpha1);
                c -= 1 - _Alpha2;
                
                return c * _Color;
            }
            
            ENDCG
        }
    }
}

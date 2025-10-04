Shader "Prototype/Mandelbrot"
{
    Properties
    {
        [NoScaleOffset] _Color("Color", 2D) = "white" {}
        _MaxIterations ("Max Iterations", Float) = 400
        _X ("X", Float) = .30077
        _Y ("Y", Float) = .02010
        _Zoom ("Zoom", Float) = .00001
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _Color;
            float _MaxIterations;
            float _X;
            float _Y;
            float _Zoom;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv - 0.5) * _Zoom + float2(_X, _Y);
                return o;
            }

            float4 mandelbrot(float cR, float cI)
            {
                float zR = cR;
                float zI = cI;
                int iteration;

                for (iteration = 0; iteration < _MaxIterations; iteration++)
                {
                    float tempZR = zR * zR - zI * zI;
                    float tempZI = 2.0f * zR * zI;
                    
                    zR = tempZR + cR;
                    zI = tempZI + cI;
                    
                    if (zR * zR + zI * zI > 4)
                        break;
                }

                float4 color = 0;

                if (iteration < _MaxIterations)
                {
                    float t = _Time.x * 10;
                    color = tex2D(_Color, float2((iteration / (float) _MaxIterations) * (_MaxIterations * 0.01) + t, 0));
                }

                return color;
            }

            float4 frag(v2f i) : SV_Target
            {
                return mandelbrot(i.uv.x, i.uv.y);
            }
            ENDCG
        }
    }
}

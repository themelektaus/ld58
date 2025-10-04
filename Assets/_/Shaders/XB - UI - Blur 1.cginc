#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest

#include "UnityCG.cginc"

struct appdata_t {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
};

struct v2f {
    float4 vertex : POSITION;
    float4 uvgrab : TEXCOORD0;
    float2 uvmain : TEXCOORD1;
};

sampler2D _MainTex;
float4 _MainTex_ST;

v2f vert(in appdata_t v)
{
    v2f o;
    
    o.vertex = UnityObjectToClipPos(v.vertex);

    #if UNITY_UV_STARTS_AT_TOP
        float scale = -1;
    #else
        float scale = 1;
    #endif

    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * .5;
    o.uvgrab.zw = o.vertex.zw;

    o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
    
    return o;
}

float _Scale;
float _Weight;
float _Size;

float getTexelSizeFactor(in float a)
{
    return a * _Scale * _Weight * (_Size * (_ScreenParams.x / 1920));
}

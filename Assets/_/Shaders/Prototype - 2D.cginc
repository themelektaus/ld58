#pragma target 2.0
#pragma multi_compile_instancing
#pragma multi_compile_local _ PIXELSNAP_ON
#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

#include "UnityCG.cginc"

#ifdef UNITY_INSTANCING_ENABLED
    
    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
        UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
    
    #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
    #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
    
#endif

CBUFFER_START(UnityPerDrawSprite)
    #ifndef UNITY_INSTANCING_ENABLED
        fixed4 _RendererColor;
        fixed2 _Flip;
    #endif
    float _EnableExternalAlpha;
CBUFFER_END

fixed4 _Color;

struct appdata_t
{
    float4 vertex    : POSITION;
    float4 color     : COLOR;
    float2 texcoord  : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    float2 worldpos : TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f SpriteVert(appdata_t IN)
{
    v2f OUT;
    
    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
    
    OUT.vertex = float4(IN.vertex.xy * _Flip, IN.vertex.z, 1);
    OUT.color = IN.color * _Color * _RendererColor;
    OUT.texcoord = IN.texcoord;
    OUT.worldpos = mul(unity_ObjectToWorld, OUT.vertex);
    
    OUT.vertex = UnityObjectToClipPos(OUT.vertex);
    #ifdef PIXELSNAP_ON
        OUT.vertex = UnityPixelSnap(OUT.vertex);
    #endif
    
    return OUT;
}

sampler2D _MainTex;
sampler2D _AlphaTex;

fixed4 SampleSpriteTexture(float2 uv)
{
    fixed4 color = tex2D(_MainTex, uv);
    
    #if ETC1_EXTERNAL_ALPHA
        fixed4 alpha = tex2D(_AlphaTex, uv);
        color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
    #endif
    
    return color;
}

fixed4 SampleSpriteTextureWithColor(in v2f IN)
{
    return SampleSpriteTexture(IN.texcoord) * IN.color;
}

fixed4 ApplyAlpha(fixed4 pixel)
{
    pixel.rgb *= pixel.a;
    return pixel;
}

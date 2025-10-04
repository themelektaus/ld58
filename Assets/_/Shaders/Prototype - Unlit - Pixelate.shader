Shader "Prototype/Unlit/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PPU ("PPU", Float) = 256
        _PPU_Ratio ("PPU Ratio", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _PPU;
            float _PPU_Ratio;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float ppu = _PPU;
                float x = round(i.uv.x * ppu) / ppu;
                ppu /= _PPU_Ratio;
                float y = round(i.uv.y * ppu) / ppu; 
                return tex2D(_MainTex, float2(x, y));
            }
            
            ENDCG
        }
    }
}

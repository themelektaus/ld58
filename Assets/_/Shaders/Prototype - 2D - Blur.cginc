float _BlurRadius;
float _BlurWeight;
float _BlurResolution;

static fixed blurAlpha[5] = {
    .2270270270,
    .1945945946,
    .1216216216,
    .0540540541,
    .0162162162
};

fixed4 SampleSpriteTextureWithColorAndBlur(in v2f IN)
{
    float2 uv = IN.texcoord;
    
    float blur = (_BlurRadius * _BlurWeight) / _BlurResolution / 8;
    
    float4 sum = 0;
    
    for (int i = -4; i <= 4; i++)
    {
        sum += tex2D(_MainTex, float2(uv.x + blur * i, uv.y + blur * i)) * blurAlpha[abs(i)];
    }
    
    sum.a = tex2D(_MainTex, uv).a;
    
    fixed4 color = IN.color;
    
    #if ETC1_EXTERNAL_ALPHA
        fixed4 alpha = tex2D(_AlphaTex, uv);
        color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
    #endif
    
    return sum * color;
}

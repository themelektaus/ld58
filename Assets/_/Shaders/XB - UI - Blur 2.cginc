
half3 preFrag(in v2f i)
{
    half3 c = 0;
    
    for (float weight = -1; weight < 1; weight += .1)
        c += grabPixel(i.uvgrab, lerp(0, 0.1, 1 - abs(weight)), weight * 4);
        
    return c;
}

half4 postFrag(in v2f i, half3 c)
{
    return half4(c, tex2D(_MainTex, i.uvmain).a);
}

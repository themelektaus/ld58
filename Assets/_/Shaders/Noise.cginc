float noise(float2 uv)
{
    return round(frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453));
}

inline float2 smoothNoiseDir(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float smoothNoise(float2 p)
{
    p /= 10;
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(smoothNoiseDir(ip), fp);
    float d01 = dot(smoothNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(smoothNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(smoothNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
}

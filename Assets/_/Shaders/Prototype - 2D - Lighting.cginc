#define PROTOTYPE_2D_LIGHTING_ENABLED
#define PROTOTYPE_2D_LIGHTING_NORMAL_ENABLED
#define PROTOTYPE_2D_LIGHTING_OCCLUSION_ENABLED
//#define PROTOTYPE_2D_LIGHTING_DEBUG_ENABLED

static const int LIGHT_CAPACITY = 20;

static const int TYPE_NONE = 0;
static const int TYPE_GLOBAL = 1;
static const int TYPE_POINT = 2;
static const int TYPE_SPOT = 3;

static const half3 SATURATION_PERMUTATION = float3(0.299, 0.587, 0.114);

int _LightingDisablePointLights;
int _LightingDisableSpotLights;
float _LightingStrength;

float4 _Prototype_2D_Lights_Position[LIGHT_CAPACITY];
half4 _Prototype_2D_Lights_Color[LIGHT_CAPACITY];
int _Prototype_2D_Lights_Type[LIGHT_CAPACITY];
float4 _Prototype_2D_Lights_Properties1[LIGHT_CAPACITY];
float4 _Prototype_2D_Lights_Properties2[LIGHT_CAPACITY];
float4 _Prototype_2D_Lights_Properties3[LIGHT_CAPACITY];

sampler2D _NormalMap;
float _NormalStrength;
float _NormalSmoothness;
float _NormalBrightness;

sampler2D _OcclusionMap;
float _OcclusionStrength;
float _OcclusionBalance;
float _OcclusionPower;

inline half3 GetNormal(float3 lightDirection, half3 normalDirection, float strength)
{
    float normal = max(0, dot(lightDirection, normalDirection));
    normal = lerp(pow(normal, max(0.01, _NormalSmoothness)), 1, normalDirection.z);
    normal += saturate(_NormalBrightness * 2 - 1) * (1 - normalDirection.z);
    return lerp(1, normal, _NormalStrength * 2 * strength);
}

void ApplyLighting_float(half4 In, float2 UV, float2 WorldPos, float Strength, out half4 Out)
{
    Out = In;

#ifdef PROTOTYPE_2D_LIGHTING_ENABLED
    if (Strength)
    {
        float3 global = Out.rgb;
#ifdef PROTOTYPE_2D_LIGHTING_DEBUG_ENABLED
        half3 globalNormal = 0;
#endif
        float3 shadow = 1;
        float3 light = 1;
        float3 lightPower = -1;

#ifdef PROTOTYPE_2D_LIGHTING_NORMAL_ENABLED
        half3 normalDirection = UnpackNormal(tex2D(_NormalMap, UV));
#endif

#ifdef PROTOTYPE_2D_LIGHTING_OCCLUSION_ENABLED
        half occlusion = tex2D(_OcclusionMap, UV);
#endif
        for (int i = 0; i < LIGHT_CAPACITY; i++)
        {
            int type = _Prototype_2D_Lights_Type[i];
            if (type == TYPE_NONE)
                continue;
            
            half4 color = _Prototype_2D_Lights_Color[i];
            float4 a = _Prototype_2D_Lights_Properties1[i];
            
            if (type == TYPE_GLOBAL)
            {
                half4 bounds = _Prototype_2D_Lights_Properties3[i];
                
                if (bounds.x == bounds.z || bounds.y == bounds.w || (
                    WorldPos.x > bounds.x && WorldPos.x < bounds.z &&
                    WorldPos.y > bounds.y && WorldPos.y < bounds.w
                ))
                {
                    float4 b = _Prototype_2D_Lights_Properties2[i];
                    
                    lightPower = abs(lightPower) * b.xyz;

                    float3 c = global * lerp(1, color.rgb, color.a);
                    c = lerp(dot(c, SATURATION_PERMUTATION), c, a.y + 1);
                    c += (c + float3(c.r * .2126, c.g * .7152, c.b * .0722)) * a.x;

#ifdef PROTOTYPE_2D_LIGHTING_NORMAL_ENABLED
                    if (_NormalStrength)
                    {
                        if (b.w)
                        {
#ifndef PROTOTYPE_2D_LIGHTING_DEBUG_ENABLED
                            half3
#endif
                            globalNormal = GetNormal(
                                normalize(float3(a.zw, 0)),
                                normalDirection,
                                b.w
                            );
                            c *= globalNormal;
                        }
                    }
#endif

                    global = c;
                }

                continue;
            }

            if (!_LightingDisablePointLights && type == TYPE_POINT)
            {
                float3 lightPos = _Prototype_2D_Lights_Position[i];

                float circle = a.x - distance(WorldPos.xy, lightPos.xy);
                a.z = pow(circle, a.z) / a.z;

                float3 add = color.rgb * max(0, circle * a.z * a.y) * color.a;

#ifdef PROTOTYPE_2D_LIGHTING_NORMAL_ENABLED
                if (_NormalStrength)
                {
                    lightPos.z *= UNITY_PI * 5;
                    float3 lightDirection = normalize(lightPos - float3(WorldPos, 0));
                    add *= GetNormal(lightDirection, normalDirection, 1);
                }
#endif

#ifdef PROTOTYPE_2D_LIGHTING_OCCLUSION_ENABLED
                if (_OcclusionStrength)
                {
                    add *= lerp(
                        1,
                        pow(occlusion * (_OcclusionBalance * 2), _OcclusionPower),
                        _OcclusionStrength
                    );
                }
#endif

                if (a.w < 1)
                {
                    shadow += add;
                }
                else
                {
                    global = max(.0001, global);
                    light += add
#ifdef PROTOTYPE_2D_LIGHTING_DEBUG_ENABLED
                        * .05
#endif
                    ;
                }
                
                continue;
            }
            
            if (!_LightingDisableSpotLights && type == TYPE_SPOT)
            {
                float3 lightPos = _Prototype_2D_Lights_Position[i];
                float2 lightDir = _Prototype_2D_Lights_Properties2[i].xy;
                float3 distance = _Prototype_2D_Lights_Properties3[i].xyz;
                
                lightPos.xy -= lightDir * a.y;
                
                float2 diff = WorldPos.xy - lightPos.xy;
                float diffLength = length(diff) - a.y;
                
                if (diffLength > 0 && diffLength < distance.x)
                {
                    float spot = dot(lightDir.xy, normalize(diff));
                    
                    if (spot > 1 - a.z && spot <= 1)
                    {
                        float fade = max(0, spot * a.x) * color.a * distance.x;
                        fade *= clamp(max(0, 1 - ((1 - spot) * (1 / a.z))) * (1 / a.w), 0, 1);
                        fade *= clamp((diffLength - distance.y) / distance.y, 0, 1);
                        fade *= clamp((distance.x - distance.z - diffLength) / distance.z, 0, 1);
                        
                        global = max(.0001, global);
                        light += color.rgb * fade;
                    }
                }
                
                continue;
            }
        }

#ifdef PROTOTYPE_2D_LIGHTING_DEBUG_ENABLED
        Out.rgb = 0;
        global = max(globalNormal - .95, .005);
        light = dot(light, SATURATION_PERMUTATION);
#else
        //global = max(.0001, global);
#endif

        Out.rgb = lerp(
            Out.rgb,
            global / shadow * lerp(
                float3(1, 1, 1),
                light,
                max(.005, lightPower)
            ),
            Strength
        );
    }

#endif
}

half4 ApplyLighting(half4 pixel, float2 texcoord, float2 worldpos)
{
    ApplyLighting_float(pixel, texcoord, worldpos, _LightingStrength, pixel);
    return pixel;
}

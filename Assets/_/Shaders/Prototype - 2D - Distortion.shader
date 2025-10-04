Shader "Prototype/2D/Distortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _AdditiveColor ("Additive Color", Vector) = (0,0,0,0)
        _AdditiveColor2 ("Additive Color 2", Vector) = (0,0,0,0)
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        
        _LightingStrength ("Lighting Strength", Range(0, 2)) = 0
        
        [KeywordEnum(UV,UV_Raw,Object,Object_Scaled,World,UI_Element,Screen)] _ShaderSpace("Shader Space", Float) = 0
        _PixelsPerUnit("Pixels Per Unit", Float) = 100
        _ScreenWidthUnits("Screen Width Units", Float) = 10
        _RectWidth("Rect Width", Float) = 100
        _RectHeight("Rect Height", Float) = 100
        [KeywordEnum(Linear_Default,Linear_Scaled,Linear_FPS,Frequency,Frequency_FPS,Custom_Value)] _TimeSettings("Time Settings", Float) = 0
        _TimeScale("Time Scale", Float) = 1
        _TimeFrequency("Time Frequency", Float) = 2
        _TimeRange("Time Range", Float) = 0.5
        _TimeFPS("Time FPS", Float) = 5
        _TimeValue("Time Value", Float) = 0
        _UVDistortFade("UV Distort: Fade", Range( 0 , 1)) = 1
        [NoScaleOffset]_UVDistortShaderMask("UV Distort: Shader Mask", 2D) = "white" {}
        _UVDistortFrom("UV Distort: From", Vector) = (-0.02,-0.02,0,0)
        _UVDistortTo("UV Distort: To", Vector) = (0.02,0.02,0,0)
        _UVDistortSpeed("UV Distort: Speed", Vector) = (2,2,0,0)
        _UVDistortNoiseScale("UV Distort: Noise Scale", Vector) = (0.1,0.1,0,0)
        _UVDistortNoiseTexture("UV Distort: Noise Texture", 2D) = "white" {}
        [HideInInspector] _texcoord( "", 2D ) = "white" {}
        
        [HideInInspector] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 10)) = 1
        _NormalSmoothness ("Normal Smoothness", Range(0, 10)) = 1
        _NormalBrightness ("Normal Brightness", Range(0, 1)) = 1
        
        _OcclusionMap ("Occlusion Map", 2D) = "bump" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1
        _OcclusionBalance ("Occlusion Balance", Range(0, 2)) = 1
        _OcclusionPower ("Occlusion Power", Range(0, 10)) = 1
    }

    SubShader
    {
        LOD 0
        
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
                
                #ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
                    #define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
                #endif
                
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 3.0
                #pragma multi_compile _ PIXELSNAP_ON
                #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
                
                #include "UnityCG.cginc"
                #include "UnityShaderVariables.cginc"
                
                #pragma shader_feature_local _SHADERSPACE_UV _SHADERSPACE_UV_RAW _SHADERSPACE_OBJECT _SHADERSPACE_OBJECT_SCALED _SHADERSPACE_WORLD _SHADERSPACE_UI_ELEMENT _SHADERSPACE_SCREEN
                #pragma shader_feature _TIMESETTINGS_LINEAR_DEFAULT _TIMESETTINGS_LINEAR_SCALED _TIMESETTINGS_LINEAR_FPS _TIMESETTINGS_FREQUENCY _TIMESETTINGS_FREQUENCY_FPS _TIMESETTINGS_CUSTOM_VALUE
                
                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                
                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color    : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                    float4 ase_texcoord1 : TEXCOORD1;
                    float4 worldpos : TEXCOORD2;
                    float4 ase_texcoord3 : TEXCOORD3;
                };
                
                uniform fixed4 _Color;
                fixed4 _AdditiveColor;
                fixed4 _AdditiveColor2;
                uniform float _EnableExternalAlpha;
                uniform sampler2D _MainTex;
                uniform sampler2D _AlphaTex;
                uniform float2 _UVDistortFrom;
                uniform float2 _UVDistortTo;
                uniform sampler2D _UVDistortNoiseTexture;
                uniform float _PixelsPerUnit;
                float4 _MainTex_TexelSize;
                uniform float _RectWidth;
                uniform float _RectHeight;
                uniform float _ScreenWidthUnits;
                uniform float2 _UVDistortSpeed;
                uniform float _TimeScale;
                uniform float _TimeFPS;
                uniform float _TimeFrequency;
                uniform float _TimeRange;
                uniform float _TimeValue;
                uniform float2 _UVDistortNoiseScale;
                uniform float _UVDistortFade;
                uniform sampler2D _UVDistortShaderMask;
                uniform float4 _UVDistortShaderMask_ST;
                
                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(IN);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                    float3 ase_worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                    OUT.worldpos.xyz = ase_worldPos;
                    float4 ase_clipPos = UnityObjectToClipPos(IN.vertex);
                    float4 screenPos = ComputeScreenPos(ase_clipPos);
                    OUT.ase_texcoord1 = IN.vertex;
                    OUT.worldpos.w = 0;
                    OUT.ase_texcoord3 = screenPos;
                    IN.vertex.xyz +=  float3(0,0,0) ; 
                    OUT.vertex = UnityObjectToClipPos(IN.vertex);
                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                    #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap (OUT.vertex);
                    #endif
                    return OUT;
                }
                
                fixed4 SampleSpriteTexture(float2 uv)
                {
                    fixed4 color = tex2D (_MainTex, uv);
                    
                    #if ETC1_EXTERNAL_ALPHA
                        fixed4 alpha = tex2D (_AlphaTex, uv);
                        color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
                    #endif

                    return color;
                }
                
                #include "Prototype - 2D - Lighting.cginc"
                
                fixed4 frag(v2f IN) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(IN);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                    
                    float2 texCoord28 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                    float2 texCoord2_g104 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                    float2 texCoord22_g104 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                    float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
                    float3 ase_worldPos = IN.worldpos.xyz;
                    float2 texCoord23_g104 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                    float2 appendResult28_g104 = (float2(_RectWidth , _RectHeight));
                    float4 screenPos = IN.ase_texcoord3;
                    float4 ase_screenPosNorm = screenPos / screenPos.w;
                    
                    ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
                    
                    #if defined(_SHADERSPACE_UV)
                        float2 staticSwitch1_g104 = ( texCoord2_g104 / ( _PixelsPerUnit * (_MainTex_TexelSize).xy ) );
                    #elif defined(_SHADERSPACE_UV_RAW)
                        float2 staticSwitch1_g104 = texCoord22_g104;
                    #elif defined(_SHADERSPACE_OBJECT)
                        float2 staticSwitch1_g104 = (IN.ase_texcoord1.xyz).xy;
                    #elif defined(_SHADERSPACE_OBJECT_SCALED)
                        float2 staticSwitch1_g104 = ( (IN.ase_texcoord1.xyz).xy * (ase_objectScale).xy );
                    #elif defined(_SHADERSPACE_WORLD)
                        float2 staticSwitch1_g104 = (ase_worldPos).xy;
                    #elif defined(_SHADERSPACE_UI_ELEMENT)
                        float2 staticSwitch1_g104 = ( texCoord23_g104 * ( appendResult28_g104 / _PixelsPerUnit ) );
                    #elif defined(_SHADERSPACE_SCREEN)
                        float2 staticSwitch1_g104 = ( ( (ase_screenPosNorm).xy * (_ScreenParams).xy ) / ( _ScreenParams.x / _ScreenWidthUnits ) );
                    #else
                        float2 staticSwitch1_g104 = ( texCoord2_g104 / ( _PixelsPerUnit * (_MainTex_TexelSize).xy ) );
                    #endif
                    
                    float mulTime5_g103 = _Time.y * _TimeScale;
                    float mulTime7_g103 = _Time.y * _TimeFrequency;
                    
                    #if defined(_TIMESETTINGS_LINEAR_DEFAULT)
                        float staticSwitch1_g103 = _Time.y;
                    #elif defined(_TIMESETTINGS_LINEAR_SCALED)
                        float staticSwitch1_g103 = mulTime5_g103;
                    #elif defined(_TIMESETTINGS_LINEAR_FPS)
                        float staticSwitch1_g103 = ( _TimeScale * ( floor( ( _Time.y * _TimeFPS ) ) / _TimeFPS ) );
                    #elif defined(_TIMESETTINGS_FREQUENCY)
                        float staticSwitch1_g103 = ( ( sin( mulTime7_g103 ) * _TimeRange ) + 100.0 );
                    #elif defined(_TIMESETTINGS_FREQUENCY_FPS)
                        float staticSwitch1_g103 = ( ( _TimeRange * sin( ( _TimeFrequency * ( floor( ( _TimeFPS * _Time.y ) ) / _TimeFPS ) ) ) ) + 100.0 );
                    #elif defined(_TIMESETTINGS_CUSTOM_VALUE)
                        float staticSwitch1_g103 = _TimeValue;
                    #else
                        float staticSwitch1_g103 = _Time.y;
                    #endif
                    
                    float2 lerpResult21_g99 = lerp( _UVDistortFrom , _UVDistortTo , tex2D( _UVDistortNoiseTexture, ( ( staticSwitch1_g104 + ( _UVDistortSpeed * staticSwitch1_g103 ) ) * _UVDistortNoiseScale ) ).r);
                    float2 appendResult2_g101 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
                    float2 uv_UVDistortShaderMask = IN.texcoord.xy * _UVDistortShaderMask_ST.xy + _UVDistortShaderMask_ST.zw;
                    float4 tex2DNode3_g102 = tex2D( _UVDistortShaderMask, uv_UVDistortShaderMask );
                    
                    fixed4 c = ( tex2D( _MainTex, ( texCoord28 + ( lerpResult21_g99 * ( 100.0 / appendResult2_g101 ) * ( _UVDistortFade * ( tex2DNode3_g102.r * tex2DNode3_g102.a ) ) ) ) ) * IN.color );
                    
                    c.rgb += _AdditiveColor.rgb * _AdditiveColor.a;
                    c.rgb += _AdditiveColor2.rgb * _AdditiveColor2.a;
                    c = ApplyLighting(c, IN.texcoord, IN.worldpos);
                    c.rgb *= c.a;
                    
                    return c;
                }
                
            ENDCG
        }
    }
}

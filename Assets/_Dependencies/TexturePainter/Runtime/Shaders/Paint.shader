Shader "Hidden/Texture Painter/Paint"
{
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
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
			};

			float4x4 mesh_Object2World;

			sampler2D _MainTex;

			float4 _Points[2];

			float4 _BrushColor;
			float _BrushSize;
			float _BrushHardness;

			v2f vert (appdata v)
			{
				v2f o;

				float2 uv = v.uv.xy;
				uv.y = 1.0 - uv.y;
				uv = uv * 2.0 - 1.0;

				o.vertex = float4(uv, 0.0, 1.0);
				o.worldPos = mul(mesh_Object2World, v.vertex);
				o.uv = v.uv;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.uv);

				for (int j = 0; j < 2; j++)
				{
					float f = distance(_Points[j].xyz, i.worldPos);
					f = 1.0 - smoothstep(_BrushSize * _BrushHardness, _BrushSize, f);
					
					float t = f * _Points[j].w * _BrushColor.a;
					color.rgb = lerp(color.rgb, _BrushColor.rgb, t);
				}

				//color.rgb = saturate(color.rgb);

				return color;
			}

			ENDCG
		}
	}
}
Shader "Sprites/Default4"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		
      	 _Speed ("Speed", Float) = 1
      	 _MinRadius ("MinRadius", Float) = 0
      	 _Intensity ("Intensity", Float) = 0
      	 _Freq ("Freq", Float) = 1

	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// #pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
					        uniform float _Freq;
      	 		uniform float _Speed;
      	 		uniform float _MinRadius;
				uniform float _Intensity;
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;



			float4 frag(v2f IN) : SV_Target
			{
	        	float xx = IN.texcoord[0] - 0.5;
	        	float yy = IN.texcoord[1] - 0.5;
	        	float dist = sqrt(xx*xx + yy*yy);
//	        	
//	        	
	        	float val = 0.5 + 0.25 * sin(_Freq * 2 * 3.14159 * dist / 0.5);
//	        	float val = 1;
				return val;
			}
		ENDCG
		}
	}
}

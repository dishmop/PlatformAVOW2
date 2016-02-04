Shader "Sprites/Default2"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		
      	 _Speed ("Speed", Float) = 1
//      	 _MinRadius ("MinRadius", Float) = 0
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
//		Blend One OneMinusSrcAlpha
		
	    	Blend SrcAlpha One // additive blending

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



			float4 frag(v2f IN) : SV_Target
			{
	        	float xx = IN.texcoord[0] - 0.5;
	        	float yy = IN.texcoord[1] - 0.5;
	        	float dist = sqrt(xx*xx + yy*yy);
	        	
	        	float distOffset0 = dist + _Speed * _Time[1];
	        	float distOffset1 = dist + 1.2 * _Speed * _Time[1];
	        	
	        	float val0 = 0.5 + 0.25 * sin(_Freq * 2 * 3.14159 * distOffset0 / 0.5);
	        	float val1 = 0.5 + 0.25 * sin(_Freq * 2.1 * 2 * 3.14159 * distOffset1 / 0.5);

	        	float distMul = min(2 * dist, 1);
	        	//return float4(1-distMul, 0, 0, 1-distMul);
	        	

				return _Intensity * float4(val0 * (1-distMul), 0, val1 * (1-distMul), 1);
			}
		ENDCG
		}
	}
}

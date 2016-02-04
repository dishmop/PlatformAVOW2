Shader "Custom/MagneticCircle" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
     
      	 _Speed ("Speed", Float) = 1
      	 _MinRadius ("MinRadius", Float) = 0
      	 _Intensity ("Intensity", Float) = 0
      	 _Freq ("Freq", Float) = 1

	}
	SubShader {
			//Cull Off
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		
			Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True" 
      		}
			
	    Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
		       // #pragma target 3.0
	
		        #include "UnityCG.cginc"
		        
		        uniform float _Freq;
      	 		uniform float _Speed;
      	 		uniform float _MinRadius;
				uniform float _Intensity;
		    
		
		        struct v2f {
		            float4 pos : SV_POSITION;
		            float2 uv : TEXCOORD0;
		        };
		
		
		        v2f vert (appdata_base v)
		        {
		            v2f o;
		            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		            o.uv = v.texcoord;
		            return o;
		        }
		        
		         
		        
		   
		        float4 frag(v2f i) : COLOR
		        {
		        	float xx = i.uv[0] - 0.5f;
		        	float yy = i.uv[1] - 0.5f;
		        	float dist = sqrt(xx*xx + yy*yy);
		        	
		        	
		        	float val = 0.5 + 0.25 * sin(_Freq * 2 * 3.14159 * dist / 0.5);
		        	
		        	return float4(val, val, val, 1);
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}

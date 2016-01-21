Shader "Custom/AVOWCursorCubeRGB" {
	Properties {
		 _Intensity ("Intensity", Float) = 1
		 _Color1 ("Color1", Color) = (1,1,1,1)
		 _Color2 ("Color2", Color) = (1,1,1,1)

	}
	SubShader {
			//Cull Off
			ZTest Always
	    	Blend SrcAlpha One // additive blending
			Tags {"Queue"="Transparent"}
			
	    Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
		        #pragma target 3.0
	
		        #include "UnityCG.cginc"
		        
		
		        uniform float  _Intensity;
				uniform float4	_Color1;
				uniform float4	_Color2; 

		    
		
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
		        
		        
		        float CalcCurve(float x, float offset)
		        {
		        	//float offset = 0.5;
		        	return 1*(1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));
	
		        }	
		        
		       	float4 CalcCol(float x, float lerpVal){		       		
	       			float4 col1 =  float4(CalcCurve(x, _Color1.r), CalcCurve(x, _Color1.g), CalcCurve(x, _Color1.b), CalcCurve(x, _Color1.a));
	       			float4 col2 =  float4(CalcCurve(x, _Color2.r), CalcCurve(x, _Color2.g), CalcCurve(x, _Color2.b), CalcCurve(x, _Color2.a));
	       			return lerp(col1, col2, lerpVal);
		       	}
		       	
		   
		        float4 frag(v2f i) : COLOR
		        {
		        	float left = abs(i.uv[0]);
		        	float right = abs(1 - i.uv[0]);
		        	float top = abs(i.uv[1]);
		        	float bottom = abs(1 - i.uv[1]);
		        	float diag = abs(i.uv[0] - i.uv[1]);
		        			        	
		        	float4 colLeft = 	CalcCol(left, i.uv[1]);
		        	float4 colRight = 	CalcCol(right, i.uv[1]);
		        	float4 colTop =		CalcCol(top, i.uv[1]);
		        	float4 colBottom = 	CalcCol(bottom, i.uv[1]);
		        	float4 colDiag = 	CalcCol(diag, i.uv[1]);
		        	
		        	return _Intensity * (colLeft + colRight +  colTop + colBottom + colDiag);
		        	
		        
	//		        	return lerp(_Color0, _Color1, val);
	
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}

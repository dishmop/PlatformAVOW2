Shader "Custom/AVOWPole" {
	Properties {
		 _v0 ("v0", Float) = 0
		 _Spacing ("Spacing", Float) = 1
      	 _Speed ("Speed", Float) = 1
      	 _Offset ("Offset", Float) = 1
      	 _Color1 ("Color1", Color) = (1, 1, 1, 1)
      	 _Color2 ("Color2", Color) = (1, 1, 1, 1)

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
				uniform float4	_UseColor;
				uniform float4	_Color1;
				uniform float4	_Color2; 
				uniform float _v0;
				uniform float _Spacing;
      	 		uniform float _Speed;
      	 		uniform float _Offset;

		    
		
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
		        
		       	float4 CalcCol(float x){		       		
	       			return float4(CalcCurve(x, _UseColor.r), CalcCurve(x, _UseColor.g), CalcCurve(x, _UseColor.b), CalcCurve(x, _UseColor.a));
		       	}
		       	
		       	
		       	// Return value between 0 and 1
		        float CalcIntensityWire(){
		        	float distToMove = 4 *(_Offset - _Time[1] * _Speed);
		        	
					float baseValue = 0.65f;

		        	
		        	float value = -sin(distToMove * 2 * 3.14159265);
		        	float scaledValue = 0.5 + value * 0.5;
		        	
		        	


		        	float yyy = (distToMove + 0.5f) % 1 - 0.5f;
		        	float x = abs(yyy);
		        	int count = round(distToMove);
		        	if (count % _Spacing != 0){
		        		return 0;
		        	}
		        	
		        	return CalcCurve(x, 140);
		        }	
		        
		       	// Return value between 0 and 1
		        float CalcIntensitySquare(){
		        	float distToMove = (_Offset - _Time[1] * _Speed);
		        	
					float baseValue = 0.65f;

		        	
		        	float value = -sin(distToMove * 2 * 3.14159265);
		        	float scaledValue = 0.5 + value * 0.5;
		        	
		        	return lerp(baseValue, 1, scaledValue);
		        	
		        }			        
		        
	
		    			       	
		       	
		   
		        float4 frag(v2f i) : COLOR
		        {
		        	
		        	_UseColor = lerp(_Color1, _Color2, _v0);
		        	_Intensity = 0.75f;
		        	
		        	
		        	float x = abs(i.uv[0] - 0.5f);
		        	float y = i.uv[1];
		        	
		        	return lerp(CalcIntensitySquare(), lerp(CalcIntensitySquare(), CalcIntensityWire(), .5), i.uv[1]) * _Intensity * CalcCol(x);

		        	
//		        	
//		        	float left = abs(i.uv[0]);
//		        	float right = abs(1 - i.uv[0]);
//		        	float top = abs(i.uv[1]);
//		        	float bottom = abs(1 - i.uv[1]);
//		        	float diag = abs(i.uv[0] - i.uv[1]);
//		        			        	
//		        	float4 colLeft = 	CalcCol(left, i.uv[1]);
//		        	float4 colRight = 	CalcCol(right, i.uv[1]);
//		        	float4 colTop =		CalcCol(top, i.uv[1]);
//		        	float4 colBottom = 	CalcCol(bottom, i.uv[1]);
//		        	float4 colDiag = 	0 * CalcCol(diag, i.uv[1]);
//		        	
//		        	return CalcIntensity() * _Intensity * (colLeft + colRight +  colTop + colBottom + colDiag);
		        	
		        
	//		        	return lerp(_Color0, _Color1, val);
	
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}

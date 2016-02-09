Shader "Custom/AVOWCursorCubeRGB2" {
	Properties {
		 _v0 ("v0", Float) = 0
		 _v1 ("v1", Float) = 1
		 _Blue ("Blue", Float) = 0
		 _Grey ("Grey", Float) = 0
		 _Spacing ("Spacing", Float) = 1
      	 _Speed ("Speed", Float) = 1
      	 _Offset ("Offset", Float) = 1
      	 _IsReversed ("IsReversed", Float) = 0
      	 
      	 _GreyCol("GreyCol", Color) = (1,1,1,1)
      	 _BlueCol("BlueCol", Color) = (1,1,1,1)
      	 _NormCol1("NormCol1", Color) = (1,1,1,1)
      	 _NormCol2("NormCol2", Color) = (1,1,1,1)
      	 
		 _InsideCol("InsideCol", Color) = (1,1,1,1)

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
				uniform float _v0;
				uniform float _v1;
				uniform float _Blue;
				uniform float _Grey;
				uniform float _Spacing;
      	 		uniform float _Speed;
      	 		uniform float _Offset;
				uniform float _IsReversed;
				uniform float4 _GreyCol;
				uniform float4 _BlueCol;
				uniform float4 _NormCol1;
				uniform float4 _NormCol2;
				uniform float4 _InsideCol;
				
		    
		
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
		       	
		       	
		       	// Return value between 0 and 1
		        float CalcIntensity(){
		        	float distToMove = (_Offset - _Time[1] * _Speed);
		        	
					float baseValue = 0.65f;

		        	
		        	float value = -sin(distToMove * 2 * 3.14159265);
		        	float scaledValue = 0.5 + value * 0.5;
		        	
		        	return lerp(baseValue, 1, scaledValue);
		        	


		        	float yyy = (distToMove + 0.5f) % 1 - 0.5f;
		        	float x = abs(yyy);
		        	int count = round(distToMove);
		        	if (count % _Spacing != 0){
		        		return baseValue;
		        	}
		        	
		        	return CalcCurve(x, 120);
		        }	
		        
	
		    			       	
		       	
		   
		        float4 frag(v2f i) : COLOR
		        {
		        
//				uniform float4 _GreyCol;
//				uniform float4 _BlueCol;
//				uniform float4 _NormCol1;
//				uniform float4 _NormCol2;
				
//						        		        
//		        	float4 col0 = float4(12f/256f, 0, 0, 1);
//		        	float4 col1 = float4(0, 12f/256f, 0, 1);
		        	
		        	float doCross = 0;
			        if (_Grey > 0.5){
			        	_Color1 = _GreyCol;
			        	_Color2 = _GreyCol;
			        	_Intensity = 0.75f;
			        }
		        	else if (_Blue > 0.5){
			        	_Color1 = _BlueCol;
			        	_Color2 = _BlueCol;
			        	_Intensity = 0.75f;
			        }
			        else{
			        	_Color1 = lerp(_NormCol1, _NormCol2, _v0);
			        	_Color2 = lerp(_NormCol1, _NormCol2, _v1);
			        	_Intensity = 0.75f;
			        	doCross =1;
			        }
		        	
		        	
		        	float left = abs(i.uv[0]);
		        	float right = abs(1 - i.uv[0]);
		        	float top = abs(i.uv[1]);
		        	float bottom = abs(1 - i.uv[1]);
		        	float diag1 = abs(i.uv[0] - i.uv[1]);
		        	float diag2 = abs(1-i.uv[1] - i.uv[0]);
		        			        	
		        	float4 colLeft = 	CalcCol(left, i.uv[1]);
		        	float4 colRight = 	CalcCol(right, i.uv[1]);
		        	float4 colTop =		CalcCol(top, i.uv[1]);
		        	float4 colBottom = 	CalcCol(bottom, i.uv[1]);
		        	float4 colDiag1 = 	doCross * _IsReversed * CalcCol(diag1, i.uv[1]);
		        	float4 colDiag2 = 	doCross * _IsReversed * CalcCol(diag2, i.uv[1]);
		        	
		        	float4 edgeCol =  0.5f * _Intensity * (colLeft + colRight +  colTop + colBottom);
		        	
		        	float4 edgeAndDiagCol =  0.8f * _Intensity * (colDiag1 + colDiag2);
		        	
		        	float4 middleCol = _InsideCol * (1-edgeCol[3]);
		        	return CalcIntensity() * (edgeCol + edgeAndDiagCol) + middleCol ;
		        	
		        
	//		        	return lerp(_Color0, _Color1, val);
	
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}

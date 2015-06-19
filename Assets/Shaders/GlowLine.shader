Shader "Custom/GlowLine" {
       Properties {
       		 _IntensityWire ("IntensityWire", Float) = 1
       		 _IntensityCase ("IntensityCase", Float) = 1
			 _ColorWire ("ColorWire", Color) = (1,1,1,1)
			 _ColorCase ("ColorCase", Color) = (1,1,1,1)
        }
        
        SubShader {
        		ZTest On
            	//Blend SrcAlpha One // additive blending
            	Cull Off
			Tags {"Queue"="Transparent"}
			
            Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
		        #pragma target 3.0
	
		        #include "UnityCG.cginc"
		        
		
		        float4 _ColorWire;
		        float4 _ColorCase;
		        
		        uniform float _IntensityWire;
		        uniform float _IntensityCase;
		    
		
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
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }	
		        
		      	float4 CalcWireColValue(float x){		       		
	       			return float4(CalcCurve(x, _ColorWire.r), CalcCurve(x, _ColorWire.g), CalcCurve(x, _ColorWire.b), CalcCurve(x, _ColorWire.a));
		       	}	
		    		        
		      	float4 CalcCaseColValue(float x){		       		
	       			return float4(CalcCurve(x, _ColorCase.r), CalcCurve(x, _ColorCase.g), CalcCurve(x, _ColorCase.b), CalcCurve(x, _ColorCase.a));
		       	}        
		       	
		       	
		        float4 CalcWireCol(v2f i){
			       	float4 col;
		        	if (i.uv[1] < 0.5f){
			        	float xx = i.uv[0] - 1;
			        	float yy = i.uv[1] - 0.5; 
			        	float dist = sqrt(xx * xx + yy * yy);
			        	float x = abs(dist - 0.25f);
			        	
			        	//col = float4(CalcCurve(x, 0.01), CalcCurve(x, 0.2), CalcCurve(x, 0.01),  CalcCurve(x, 0.02));
			        	col = CalcWireColValue(x);
			        }
			        else{
			        	float xx = i.uv[0] - 0.75;
			        	float yy = i.uv[1] - 0.5;
			        	float x = sqrt(xx * xx + yy * yy);
			        	
			        	col = CalcWireColValue(x);
			        }
			        return col * _IntensityWire;
		        }
		        
		        float4 CalcCaseCol(v2f i){
		        
		        	float left = abs(i.uv[0]-0.5f);
		        	float right = abs(1 - i.uv[0]);
		        	if (i.uv[1] < 0.5f){
			        	float xx = i.uv[0] - 1;
			        	float yy = i.uv[1] - 0.5; 
		        		right = sqrt(xx * xx + yy * yy);
		        	}
		        	float top = abs(i.uv[1]);
		        	float bottom = abs(0.75f - i.uv[1]);
		        			        	
		        	float4 colLeft = 	CalcCaseColValue(left);
		        	float4 colRight = 	CalcCaseColValue(right);
		        	float4 colTop =		CalcCaseColValue(top);
		        	float4 colBottom = 	CalcCaseColValue(bottom);
		        	
		        	return _IntensityCase * (colLeft + colRight +  colTop + colBottom);
		        }
		        

		        float4 frag(v2f i) : COLOR
		        {
		     	   return CalcWireCol(i) + CalcCaseCol(i);

		        	
		        
//		        	return lerp(_Color0, _Color1, val);

		        }
		        
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }

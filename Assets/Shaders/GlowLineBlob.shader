Shader "Custom/GlowLineBlob" {
       Properties {
      		 _Spacing ("Spacing", Float) = 1
      		 _Speed ("Speed", Float) = 1
      		 _Offset ("Offset", Float) = 1
       		 _IntensityWire ("IntensityWire", Float) = 1
			 _ColorWire ("ColorWire", Color) = (1,1,1,1)
        }
        
        SubShader {
        		ZTest On
            //	Blend SrcAlpha One // additive blending
            	Cull Off
			Tags {"Queue"="Transparent"}
			
            Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
		        #pragma target 3.0
	
		        #include "UnityCG.cginc"
		        
		
		        float4 _ColorWire;
		        
		        uniform float _IntensityWire;
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
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }	
		        
		      	float4 CalcWireColValue(float x){		       		
	       			return float4(CalcCurve(x, _ColorWire.r), CalcCurve(x, _ColorWire.g), CalcCurve(x, _ColorWire.b), CalcCurve(x, _ColorWire.a));
		       	}	
		    		        
	       	
		       	
		        float4 CalcWireCol(v2f i){
		        	float distToMove = _Offset - _Time[1] * _Speed;
					int intDist = round(distToMove);
					if (intDist < 0){
		            	distToMove -= intDist;
		            }

			       	float4 col = float4(0, 0, 0, 0);
			       	float xx = i.uv[0] * 4;
			       	float yy = (i.uv[1] + distToMove) * 4;
			       	
			       	
			       	
		        	if (xx  > 1){
		        		return float4(0, 0, 0, 1);
			        }
			        else{
			        	float xxx = xx - 0.5f;
			        	float yyy = (yy + 0.5f) % 1 - 0.5f;
			        	float x = sqrt(yyy * yyy + xxx * xxx);//sqrt(xx * xx + yy * yy);
			        	int count = round(yy);
			        	if (count % _Spacing != 0){
			        		return float4(0, 0, 0, 1);
			        	}
			        	
			        	col = CalcWireColValue(abs(x));
			        }
			        return col * _IntensityWire;
		        }
		        
		        

		        float4 frag(v2f i) : COLOR
		        {
		     	   return CalcWireCol(i);

		        	
		        
//		        	return lerp(_Color0, _Color1, val);

		        }
		        
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }

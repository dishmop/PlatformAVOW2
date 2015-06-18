Shader "Custom/GlowLine" {
       Properties {
       		_Intensity ("Intensity", Float) = 1
			 _Color ("Color", Color) = (1,1,1,1)
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
		        
		
		        float4 _Color;
		        
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
		        
		        
		        float CalcCurve(float x, float offset)
		        {
		        	//float offset = 0.5;
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }	
		        
		      	float4 CalcCol(float x){		       		
	       			return float4(CalcCurve(x, _Color.r), CalcCurve(x, _Color.g), CalcCurve(x, _Color.b), CalcCurve(x, _Color.a));
		       	}	        

		        float4 frag(v2f i) : COLOR
		        {
		        	float4 col;
		        	if (i.uv[1] < 0.5f){
			        	float xx = i.uv[0] - 1;
			        	float yy = i.uv[1] - 0.5; 
			        	float dist = sqrt(xx * xx + yy * yy);
			        	float x = abs(dist - 0.25f);
			        	
			        	//col = float4(CalcCurve(x, 0.01), CalcCurve(x, 0.2), CalcCurve(x, 0.01),  CalcCurve(x, 0.02));
			        	col = CalcCol(x);
			        }
			        else{
			        	float xx = i.uv[0] - 0.75;
			        	float yy = i.uv[1] - 0.5;
			        	float x = sqrt(xx * xx + yy * yy);
			        	
			        	col = CalcCol(x);
			        }
			        return col * _Intensity;
		        	
		        
//		        	return lerp(_Color0, _Color1, val);

		        }
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }

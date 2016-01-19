using UnityEngine;
using System.Collections;
using System.Linq;

public class AVOWGrid : MonoBehaviour {
	public GameObject background;
	
	public float resistance = 1;
	public float maxVoltage = 1;
	public float vAToRealSize = 1f;		// Size of 1v or 1A in real space
	public float borderSize = 0.1f;		// Size of border in real world 
	public float gradScale = -10f;
	public float minVoltage = 0.5f;
	
	
	float checkSum = 0;
	
	
	// Derived measurementes
	float realSize;				// Width or height of full texture in world units
	float maxCurrent;
	float minCurrent;
	float vaRectWidth;
	float vaRectHeight;
	float quadWidth;
	float quadHeight;
	float quadTextUMax;
	float quadTextVMax;
	
	
	const int textSize = 512;
	const float penGrad = 1;		// How much deep you go for each world unit of radius
	float maxAbsHeight = 0.00001f;
	Texture2D gridDifffuse;
	Texture2D gridNormal;
	Color[] diffuseCols;
	Color[] normalCols;
	float[] heights;
	float[] gradMags;
	float[] grad2Mags;
	Vector2[] grads;
	
	float[,] sobelH = new float[3, 3]{ {-0.125f, -0.25f, -0.125f}, {0f, 0f, 0f}, {0.125f, 0.25f, 0.125f} };
	float[,] sobelV = new float[3, 3]{ {-0.125f, 0f, 0.125f}, {-0.25f, 0f, 0.25f}, {-0.125f, 0f, 0.125f} };
	float[,] sobel2H = new float[3, 3];
	float[,] sobel2V = new float[3, 3];
	float[,] guassian = new float[3, 3]{ {0.0625f, 0.125f, 0.0625f}, {0.125f, 0.25f, 0.125f}, {0.0625f, 0.125f, 0.0625f} };
	float[,] unit = new float[3, 3]{ {0, 0f, 0f}, {0f, 1f, 0f}, {0f, 0f, 0f} };
	
	
	class LineV{
		public Vector2 startPos;
		public Vector2 endPos;
		public float radius;
		public float depth;
		
		
		public LineV(Vector2 startPos, Vector2 endPos, float radius, float depth){
			this.startPos = startPos;
			this.endPos = endPos;
			this.radius = radius;
			this.depth = depth;
		}
		
		public float CalcHeight(Vector2 pos){
			// first get the nearest point to pos on the line
			Vector2 startToEnd = endPos - startPos;
			
			float lambda = Vector2.Dot((pos - startPos), startToEnd) / startToEnd.sqrMagnitude;
			Vector2 nearestPos = Vector2.zero;
			if (lambda < 0){
				nearestPos = startPos;
			}
			else if (lambda > 1){
				nearestPos = endPos;
			}
			else{
				nearestPos = startPos + lambda * startToEnd;
			}
			
			// If we are outside the range, then easy just return the level ground
			float sqDist = (nearestPos - pos).sqrMagnitude;
			if (sqDist > radius * radius){
				return 0;
			}
			
			float chamferGrad = depth / radius;
			float retVal = -chamferGrad * (radius-Mathf.Sqrt(sqDist));
			return retVal;
			
		}
	}
	
	// Use this for initialization
	void Start () {
	
		CreateTextures();
		AssignTexturesToMaterial();
		
		Recalc ();
		checkSum = CalcCheckSum();
		

		
	
	}
	
	float CalcCheckSum(){
		return resistance + 
			   maxVoltage + 
			   vAToRealSize + 
			   borderSize + 
			   gradScale +
			   minVoltage;
	}
		
		
	
	void CalcTextureDimensions(){

		// Derived measurementes
		maxCurrent = maxVoltage / resistance;
		minCurrent = minVoltage / resistance;
		
		vaRectWidth = maxCurrent * vAToRealSize;
		vaRectHeight = maxVoltage * vAToRealSize;
		
		quadWidth = vaRectWidth + 2 * borderSize;
		quadHeight = vaRectHeight + 2 * borderSize;
		
		if (quadWidth > quadHeight){
			quadTextUMax = 1;
			realSize = quadWidth;
			quadTextVMax = quadHeight / quadWidth;
		}
		else{
			quadTextVMax = 1;
			realSize = quadHeight;
			quadTextUMax = quadWidth / quadHeight;
		}
	}
	
	void ReconfigureMesh(){
		Mesh quadMesh = background.GetComponent<MeshFilter>().mesh;
//		for (int i = 0; i < quadMesh.vertices.Length; ++i){
//			Debug.Log("quadMesh.vertices[" + i + "] = " + quadMesh.vertices[i].ToString());
//		}
//
//		for (int i = 0; i < quadMesh.vertices.Length; ++i){
//			Debug.Log("quadMesh.uv[" + i + "] = " + quadMesh.uv[i].ToString());
//		}
		
		float halfWidth = quadWidth * 0.5f;
		float halfHeight = quadHeight * 0.5f;
		
		Vector3[] newVerts = new Vector3[4];
		newVerts[0] = new Vector3(-halfWidth, -halfHeight, 0);
		newVerts[1] = new Vector3(halfWidth, halfHeight, 0);
		newVerts[2] = new Vector3(halfWidth, -halfHeight, 0);
		newVerts[3] = new Vector3(-halfWidth, halfHeight, 0);
		
		Vector2[] newUVs = new Vector2[4];
		newUVs[0] = new Vector2(0, 0);
		newUVs[1] = new Vector2(quadTextUMax, quadTextVMax);
		newUVs[2] = new Vector2(quadTextUMax, 0);
		newUVs[3] = new Vector2(0, quadTextVMax);
		
		
		quadMesh.vertices = newVerts;
		quadMesh.uv = newUVs;
		quadMesh.UploadMeshData(false);
		
		
	}
	
	void Compute2ndOrderSobel(){
		for (int y = 0; y < 3; y++){
			for (int x = 0; x < 3; x++){
				sobel2H[x,y] = sobelH[x,y] * sobelH[x,y];
				sobel2V[x,y] = sobelV[x,y] * sobelV[x,y];
				
			}
		}
		
		sobel2H = new float[3,3] { {0, 0.125f, 0}, {0.125f, -0.5f, 0.125f}, {0, 0.125f, 0} };

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float newCheckSum = CalcCheckSum();
		if (newCheckSum != checkSum){
			Recalc();
			checkSum = newCheckSum;
		}
	
	}
	
	void Recalc(){
		Compute2ndOrderSobel();
		CalcTextureDimensions();
		ReconfigureMesh();
		
		FillTextures();
		float boxRadius = 0.015f;
		float intAxisRadius = 0.015f;
		
		
		// Outer box
		float boarderSizeBA = borderSize / vAToRealSize;
		
		Vector2 blQuad = new Vector2(-boarderSizeBA, -boarderSizeBA);
		Vector2 brQuad = new Vector2(maxCurrent + boarderSizeBA, -boarderSizeBA);
		Vector2 trQuad = new Vector2(maxCurrent + boarderSizeBA, maxVoltage + boarderSizeBA);
		Vector2 tlQuad = new Vector2(-boarderSizeBA, maxVoltage + boarderSizeBA);
				
		DrawSquareVA(blQuad, brQuad, trQuad, tlQuad, boxRadius, false);
		
		
		
		// VA box
		Vector2 bl = new Vector2(0f, 0f);
		Vector2 br = new Vector2(maxCurrent, 0f);
		Vector2 tr = new Vector2(maxCurrent, maxVoltage);
		Vector2 tl = new Vector2(0f, maxVoltage);
		
		
		DrawSquareVA(bl, br, tr, tl, boxRadius, false);

//		DrawLineVA(bl, tr, boxRadius, false);



		
		float maxProp = 0f;
		DrawGridLines(1, maxProp, boxRadius);
		DrawGridLines(0.5f, maxProp*0.5f, intAxisRadius);
		DrawGridLines(0.25f, maxProp*0.25f, intAxisRadius * 0.5f);
		DrawGridLines(0.125f, maxProp*0.125f, intAxisRadius * 0.25f);
		
		// Do minimum voltage - if we have one
		Vector2 blMin = new Vector2(0f, 0f);
		Vector2 brMin = new Vector2(minCurrent, 0f);
		Vector2 trMin = new Vector2(minCurrent, minVoltage);
		Vector2 tlMin = new Vector2(0f, minVoltage);
		
		Vector2 centre = new Vector2((maxCurrent - minCurrent) * 0.5f, (maxVoltage - minVoltage) * 0.5f);
		
		
		DrawFilledSquareVA(centre + blMin, 
		             centre + brMin, 
		             centre + trMin, 
		             centre + tlMin, 
		             boxRadius, 
		             false);
		
		
		ConvertHeightToNormal();
		UploadTextureData();
		
	}
	
	
	void DrawGridLines(float step, float protrudePropBoarder, float radius){
	
		float vaProtrude = protrudePropBoarder * borderSize / vAToRealSize;
		Vector2 centre = new Vector2(maxCurrent * 0.5f, maxVoltage * 0.5f);
		// Vertical centre line
		DrawLineVA( new Vector2(centre.x, -vaProtrude), new Vector2(centre.x, maxVoltage + vaProtrude), radius, false);
		// Horizontal central line
		DrawLineVA( new Vector2(-vaProtrude, centre.y), new Vector2(maxCurrent+ vaProtrude, centre.y), radius, false);
		
		int numVLines = Mathf.FloorToInt(maxCurrent * 0.5f / step);
		for (int i = 0; i < numVLines; ++i){
			DrawLineVA( new Vector2(centre.x + (i + 1) * step, -vaProtrude), new Vector2(centre.x + (i + 1) * step, maxVoltage + vaProtrude), radius, false);
			DrawLineVA( new Vector2(centre.x - (i + 1) * step, -vaProtrude), new Vector2(centre.x - (i + 1) * step, maxVoltage + vaProtrude), radius, false);
			
		}
		
		int numHLines = Mathf.FloorToInt(maxVoltage * 0.5f / step);
		for (int i = 0; i < numHLines; ++i){
			DrawLineVA( new Vector2(-vaProtrude, centre.y + (i + 1) * step), new Vector2(maxCurrent+ vaProtrude, centre.y + (i + 1) * step), radius, false);
			DrawLineVA( new Vector2(-vaProtrude, centre.y - (i + 1) * step), new Vector2(maxCurrent+ vaProtrude, centre.y - (i + 1) * step), radius, false);
			
		}
	}
	
	
	void CreateTextures(){
		gridDifffuse  = new Texture2D(textSize, textSize, TextureFormat.ARGB32, true);
		gridNormal = new Texture2D(textSize, textSize, TextureFormat.ARGB32, true);
		
		diffuseCols = new Color[textSize * textSize];
		normalCols = new Color[textSize * textSize];
		heights = new float[textSize * textSize];
		gradMags = new float[textSize * textSize];
		grad2Mags = new float[textSize * textSize];
	}
	
	void FillTextures(){
	
		Color initCol = new Color(0.5f, 0.5f, 0.5f, 1);
	
		for (int i = 0; i < textSize * textSize; ++i){
			diffuseCols[i] = initCol;
			normalCols[i] = new Color(0f, 0f, 0f, 0f);
			heights[i] = 0;
			
		}
	}
	
	void UploadTextureData(){
		gridDifffuse.SetPixels(diffuseCols);
		gridDifffuse.Apply();
		
		gridNormal.SetPixels(normalCols);
		gridNormal.Apply();
	}
	
	void AssignTexturesToMaterial(){
		Material gridMat = background.GetComponent<MeshRenderer>().material;
		gridMat.SetTexture("_MainTex", gridDifffuse);
		gridMat.SetTexture("_BumpMap", gridNormal);
		
	}
	
	// Convert from a voltage and current to a positio in on the grid
	Vector2 TransformVAToReal(Vector2 pos){
		return pos * vAToRealSize + new Vector2(borderSize, borderSize);
	}
	
	Vector2 TransformRealToVA(Vector2 pos){
		return (pos - new Vector2(borderSize, borderSize)) / vAToRealSize;
	}
	
	void DrawSquareVA(Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, float radius, bool raise){
		DrawLineVA(bl, br, radius, raise);
		DrawLineVA(br, tr, radius, raise);
		DrawLineVA(tr, tl, radius, raise);
		DrawLineVA(tl, bl, radius, raise);
	}
	
	void DrawFilledSquareVA(Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, float radius, bool raise){
		Vector2 blTrans = TransformVAToReal(bl);
		Vector2 brTrans = TransformVAToReal(br);
		Vector2 trTrans = TransformVAToReal(tr);
		Vector2 tlTrans = TransformVAToReal(tl);
		
		DrawLine(blTrans, brTrans, radius, raise);
		DrawLine(brTrans, trTrans, radius, raise);
		DrawLine(trTrans, tlTrans, radius, raise);
		DrawLine(tlTrans, blTrans, radius, raise);
		float depth = radius * penGrad;
		
		Vector2 pixelBL = textSize * blTrans / realSize;
		Vector2 pixelTR = textSize * trTrans / realSize;
		
		int i = 0;
		for (int y = 0; y < textSize; ++y){
			for (int x = 0; x < textSize; ++x){
				if (x > pixelBL.x && x < pixelTR.x && y > pixelBL.y && y < pixelTR.y){
					if (raise){
						heights[i] = Mathf.Max(heights[i], depth);
					}
					else{
						heights[i] = Mathf.Min(heights[i], -depth);
					}
				}

				++i;
				
			}
		}
	}
	
	void DrawLineVA(Vector2 startPos, Vector2 endPos, float radius, bool raise){
		DrawLine(TransformVAToReal(startPos), TransformVAToReal(endPos), radius, raise);
	}
	
	// All arguments in real world units (m)
	void DrawLine(Vector2 startPos, Vector2 endPos, float radius, bool raise){
//		Debug.Log ("DrawLine(" + startPos + ", " + endPos + ");");
		Vector2 pixelStartPos = textSize * startPos / realSize;
		Vector2 pixelEndPos = textSize * endPos / realSize;
		float pixelRadius = textSize * radius / realSize;
		
		LineV line = new LineV(pixelStartPos, pixelEndPos, pixelRadius, radius * penGrad);
		
		int i = 0;
		for (int y = 0; y < textSize; ++y){
			for (int x = 0; x < textSize; ++x){
				float retHeight = line.CalcHeight(new Vector2(x, y));
				maxAbsHeight = Mathf.Max(maxAbsHeight, Mathf.Abs(retHeight));
				if (raise){
					heights[i] = Mathf.Max(heights[i], -retHeight);
				}
				else{
					heights[i] = Mathf.Min(heights[i], retHeight);
				}
				++i;
				
			}
		}
	}
	
	
	//	http://answers.unity3d.com/questions/47121/runtime-normal-map-import.html
	
	void ConvertHeightToNormal(){
		int i = 0;
		for (int y = 0; y < textSize; ++y){
			for (int x = 0; x < textSize; ++x){
				Vector3 normal = CalcGradAtCoord(x, y);

			//	grad2Mags[i] = Mathf.Clamp(CalcFilterResult(x, y, heights,sobel2H) * textSize / realSize, -0.5f, 0.5f);
				grad2Mags[i] = CalcFilterResult(x, y, heights,sobel2H) * textSize / realSize;
				


	// This needs a different layout if on mobile!
				normalCols[i] = new Color(0, 
				                          0.5f + normal.y * 0.5f, 
				                          0, 
				                          0.5f + normal.x * 0.5f);
				++i;
				
			}
		}
		i = 0;
		for (int y = 0; y < textSize; ++y){
			for (int x = 0; x < textSize; ++x){
				gradMags[i] = CalcFilterResult(x, y, grad2Mags, guassian);
				
				++i;
				
			}
		}	
		i = 0;
		for (int y = 0; y < textSize; ++y){
			for (int x = 0; x < textSize; ++x){
				grad2Mags[i] = CalcFilterResult(x, y, gradMags,guassian);
				
				grad2Mags[i] = gradScale * grad2Mags[i];
				
				diffuseCols[i] = new Color(0.5f + grad2Mags[i] * 0.5f, 
				                           0.5f + grad2Mags[i] * 0.5f, 
				                           0.5f + grad2Mags[i] * 0.5f, 
				                           1);
				
				
				++i;
				
			}
		}		
		
		
	}
	

	
	Vector3 CalcGradAtCoord(int x, int y){
		
		Vector3 xGrad = new Vector3(1, 0, CalcFilterResult(x, y, heights,sobelH) * textSize / realSize);
		Vector3 yGrad = new Vector3(0, 1, CalcFilterResult(x, y, heights,sobelV) * textSize / realSize);
		Vector3 normal = Vector3.Cross(xGrad, yGrad).normalized;
		return normal;
	}
	
	float CalcFilterResultQuick(int x, int y, float[] image, float[,] kernel){
		int i = y * textSize + x;
		float result = image[i - 1 - textSize] 	* kernel[0, 0] +	 
				image[i - textSize] 		* kernel[0, 1] +	 
				image[i + 1 - textSize] 	* kernel[0, 2] +	 
				
				image[i - 1] 	* kernel[1, 0] +	 
				image[i] 		* kernel[1, 1] +	 
				image[i + 1] 	* kernel[1, 2] +	
				
				image[i - 1 + textSize] 	* kernel[2, 0] +	 
				image[i + textSize] 		* kernel[2, 1] +	 
				image[i + 1 + textSize] 	* kernel[2, 2];
		return result;
	}
	
	float CalcFilterResult(int x, int y, float[] image, float[,] kernel){
		
		float result = 0;
		for (int yy = 0; yy < 3; ++yy){
			for (int xx = 0; xx < 3; ++xx){
				int xCord = Mathf.Max (0, Mathf.Min (textSize - 1, x - 1 + xx));
				int yCord = Mathf.Max (0, Mathf.Min (textSize - 1, y - 1 + yy));
				int i = yCord * textSize + xCord;
				result += image[i] * kernel[xx, yy];
				
			}
		}
		return result;
	}
	
		
}

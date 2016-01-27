using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WireLine : MonoBehaviour {

	public float width;
	
	public enum EndType{
		kContinue,
		kEnd
	}
	
	public EndType end0;
	public EndType end1;
	
	public Color	wireColor;
	public float 	wireIntensity;
	public float	speed;
	public float	offset;
	
	public float wireLength;
	
	public Vector3[] points;
	
	
	// For internal storage of vertex data
	
	int numTrisInCorner = 20;
	
	// Useful numbers
	int numCorePoints;
	int numCoreLines;
	int numJunctions;
	int numVertices;
	int numUVs;
	int numTris;
	int numTriIndices;
	
	// Internal storage of mesh data
	Vector3[] newVertices;
	Vector2[] newUV;
	int[] newTriangles;
	
	bool pointsDirtyFlag = true;
	
	
	public void SetNewPoints(Vector3[] newPoints){
		points = newPoints;
		pointsDirtyFlag = true;
	}
	
	// Sets the speed up and and adjusts the offset
	// so there is not a big jump
	public void SetSpeed(float newSpeed){
		if (MathUtils.FP.Feq (newSpeed, speed)){
			speed = newSpeed;
			return;
		}
		
		float distToMove = offset - Time.timeSinceLevelLoad * speed;

		
		// Now we want to figure out a new offset in order that the new speed starts with the same internal offset
		offset = distToMove +  Time.timeSinceLevelLoad * newSpeed;
		speed = newSpeed;
	}
	
	
	public bool IsPointInside(Vector3 point, out float distAlong){
		
		if (pointsDirtyFlag){
			ConstructMesh();
		}
		
		// Every set of 4 vertices is an axis aligned quad
		// I.e. mega trivial to determin if we are inside
		distAlong = 0;
		float cumDistAlong = 0;
		
		point.z = transform.TransformPoint(points[0]).z;
		// Every set of 4 vertices is an axis aligned quad
		float nearestDist = 100000f;
		for (int i = 0; i < points.Length - 1; ++i){
			// Do one line segment at a time
			Vector3 startPos = transform.TransformPoint(points[i]);
			Vector3 endPos = transform.TransformPoint(points[i+1]);
			
			// first get the nearest point to pos on the line
			Vector3 startToEnd = endPos - startPos;
			
			
			float lambda = Vector3.Dot((point - startPos), startToEnd) / startToEnd.sqrMagnitude;
			
			Vector3 thisNearestPos;
			if (lambda < 0){
				thisNearestPos = startPos;
			}
			else if (lambda > 1){
				thisNearestPos = endPos;
			}
			else{
				thisNearestPos = startPos + lambda * startToEnd;
			}
			
			// If we are outside the range, then easy just return the level ground
			float thisDist = (thisNearestPos - point).magnitude;
			
			if (thisDist < nearestDist){
				nearestDist= thisDist;
				distAlong = cumDistAlong + (startPos - thisNearestPos).magnitude;
			}
			cumDistAlong += startToEnd.magnitude;
			
		}
		
		return nearestDist < width * 0.5f;
		
		
	}		
		
	
	public float CalMinDistToWire(Vector3 pos, out Vector3 nearestPos){
		nearestPos = Vector2.zero;
		if (pointsDirtyFlag){
			ConstructMesh();
		}
		
		pos.z = transform.TransformPoint(points[0]).z;
		// Every set of 4 vertices is an axis aligned quad
		float nearestDist = 100000f;
		for (int i = 0; i < points.Length - 1; ++i){
			// Do one line segment at a time
			Vector3 startPos = transform.TransformPoint(points[i]);
			Vector3 endPos = transform.TransformPoint(points[i+1]);
			
			// first get the nearest point to pos on the line
			Vector3 startToEnd = endPos - startPos;
			
			float lambda = Vector3.Dot((pos - startPos), startToEnd) / startToEnd.sqrMagnitude;
			
			Vector3 thisNearestPos;
			if (lambda < 0){
				thisNearestPos = startPos;
			}
			else if (lambda > 1){
				thisNearestPos = endPos;
			}
			else{
				thisNearestPos = startPos + lambda * startToEnd;
			}
			
			// If we are outside the range, then easy just return the level ground
			float thisDist = (thisNearestPos - pos).magnitude;
			
			if (thisDist < nearestDist){
				nearestDist= thisDist;
				nearestPos = thisNearestPos;
			}
			
		}

		return nearestDist;

		
	}
	
	void ConstructMesh(){
	
		SimplifyLine();
		CalcArraySizes();
		ConstructArrays();
		FillArrays();
		AssignMeshVars();
		SetupMaterials();
		
		pointsDirtyFlag = false;
		
	}
	
	

	// Use this for initialization
	void Start () {
		// need to create the mesh initially
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		
		// Ensure it is read/writable
		mesh.UploadMeshData(false);
				
		ConstructMesh();
	
	}
	
	// Update is called once per frame
	void Update () {
		ConstructMesh();
	
	}
	
	// Ensure it is all a series of horizontal and vertical lines
	void SimplifyLine(){
		wireLength = 0;
		
		// If no points then no simplification
		if (points.Length < 2){
			Debug.Log("WireLine not enough points: " + GetInstanceID());
			return;
		}
		
		// Make a temporary list to store our simplified points and add the first one
		List<Vector3> tempList = new List<Vector3>();
		tempList.Add (points[0]);
		
		// Now only add a new point if the direction changes - otherwise replace the last entry
		int lastDir = Directions.kNull;
		for (int i = 0; i < points.Length - 1; ++i){
			wireLength += (points[i+1] - points[i]).magnitude;
			
			int thisDir = Directions.GetStrictDirection(points[i], points[i+1]);
			if (thisDir != Directions.kNull){
				if (thisDir != lastDir){
					tempList.Add (points[i+1]);
				}
				else{
					tempList[tempList.Count-1] = points[i+1];
				}
				lastDir = thisDir;
			}
		}
		if (tempList.Count() < 2){
			Debug.Log("WireLine not enough points: " + GetInstanceID());
			return;
		}
		
		points = tempList.ToArray();
	
	}

	
	void CalcArraySizes(){
		// Set up number of elements in each array for the mesh
		numCorePoints = points.Length;
		numCoreLines = numCorePoints - 1;
		numJunctions = numCoreLines - 1;
		
		// Four points for each line segmen8t and then 6 points at the junction to complete the corner square
		numVertices = 4 * numCoreLines + numTrisInCorner * 3 * numJunctions;
		numUVs = numVertices;
		numTris = 2 * numCoreLines + numTrisInCorner * numJunctions;
		numTriIndices = 3 * numTris;
	}
	
	void ConstructArrays(){
		if (numVertices < 3){
			Debug.Log ("Not enough vertices");
		}
		// Set up the arrays themselves
		newVertices = new Vector3[numVertices];
		newUV = new Vector2[numUVs];
		newTriangles = new int[numTriIndices];
		
	}
	
	void FillArrays(){
		int vertexIndex = 0;
		int uvIndex = 0;
		int triIndex = 0;
		
		float startLen = 0;
		float endLen= -1;
				
		for (int i = 0; i < points.Length-1; ++i){
		
			// End points of core line
			Vector3 point0 = points[i];
			Vector3 point1 = points[i + 1];
			
			Vector3 lenVec = 0.5f * width * (point1 - point0).normalized;
			
			// Adjust the end positions in byb half a width if not an end point
			if (i != 0){
				point0 += lenVec;
			}
			else{
			}
			if (i != points.Length-2){
				point1 -= lenVec;
			}
			endLen = startLen +  (point0 - point1).magnitude * 0.25f / width;
			
			
			// half-width Vector perpendicular to core line
			Vector3 sideVec =  0.5f * width * Vector3.Cross(point1 - point0, new Vector3(0, 0, 1)).normalized;
			int firstVertexIndex = vertexIndex;
			
			// The quad
			newVertices[vertexIndex++] = point0 - sideVec;
			newVertices[vertexIndex++] = point0 + sideVec;
			newVertices[vertexIndex++] = point1 - sideVec;
			newVertices[vertexIndex++] = point1 + sideVec;
			
			// Just use the thin band accross the middle of the texture
			newUV[uvIndex++] = new Vector2(0f, startLen);
			newUV[uvIndex++] = new Vector2(0.25f, startLen);
			newUV[uvIndex++] = new Vector2(0f, endLen);
			newUV[uvIndex++] = new Vector2(0.25f, endLen);
			
			// Tri 0
			newTriangles[triIndex++] = firstVertexIndex + 0;
			newTriangles[triIndex++] = firstVertexIndex + 3;
			newTriangles[triIndex++] = firstVertexIndex + 1;
			
			// Tri 1
			newTriangles[triIndex++] = firstVertexIndex + 0;
			newTriangles[triIndex++] = firstVertexIndex + 2;
			newTriangles[triIndex++] = firstVertexIndex + 3;
			
			startLen = endLen;
			
			// If point1 is NOT the last one - then making a joining quad
			if (i+1 != points.Length-1){
			
				
				int firstJoinVertexIndex = vertexIndex;
			
				// Length of this curved semgent
				//float segmentLen = 0.5f * Mathf.PI * sideVec.magnitude;
				
				// Use this method because easier to keep track off offset down wire
				// (simply a funciton of its length).
				float segmentLen = 2f * sideVec.magnitude;
				
				endLen = startLen +  segmentLen * 0.25f / width;
				
				// The quad

				
				// Figure out if we are turning clockwise of anticlockwise
				Vector3 point2 = points[i + 2];
				float turnRes = Vector3.Dot(point2 - point1, sideVec);
				
				
				// If going clockwise
				if (turnRes > 0){
										
					for (int j = 0; j < numTrisInCorner; ++j){
						float ang0 = j * Mathf.PI * 0.5f / numTrisInCorner;
						float ang1 = (j + 1) * Mathf.PI * 0.5f / numTrisInCorner;
						Vector3 centre = point1 + sideVec;
						
						
						newVertices[vertexIndex++] = centre;
						newVertices[vertexIndex++] = centre + 2 * lenVec * Mathf.Sin(ang0) - 2 * sideVec * Mathf.Cos (ang0);
						newVertices[vertexIndex++] = centre + 2 * lenVec * Mathf.Sin(ang1) - 2 * sideVec * Mathf.Cos (ang1);
						
						newUV[uvIndex++] = new Vector2(0.25f, Mathf.Lerp(startLen, endLen, ang0 / (Mathf.PI * 0.5f)));
						newUV[uvIndex++] = new Vector2(0f, Mathf.Lerp(startLen, endLen, ang1 / (Mathf.PI * 0.5f)));
						newUV[uvIndex++] = new Vector2(0f, Mathf.Lerp(startLen, endLen, ang1 / (Mathf.PI * 0.5f)));
					}
					
				
					
//					newUV[uvIndex++] = new Vector2(1, 0.5f);
//					newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
//					newUV[uvIndex++] = new Vector2(1, 0);
//					newUV[uvIndex++] = new Vector2(0.5f, 0);
				}
				else{
					for (int j = 0; j < numTrisInCorner; ++j){
						float ang0 = j * Mathf.PI * 0.5f / numTrisInCorner;
						float ang1 = (j + 1) * Mathf.PI * 0.5f / numTrisInCorner;
						Vector3 centre = point1 - sideVec;
						
						
						newVertices[vertexIndex++] = centre;
						newVertices[vertexIndex++] = centre + 2 * lenVec * Mathf.Sin(ang0) + 2 * sideVec * Mathf.Cos (ang0);
						newVertices[vertexIndex++] = centre + 2 * lenVec * Mathf.Sin(ang1) + 2 * sideVec * Mathf.Cos (ang1);
						
						newUV[uvIndex++] = new Vector2(0.25f, Mathf.Lerp(startLen, endLen, ang0 / (Mathf.PI * 0.5f)));
						newUV[uvIndex++] = new Vector2(0f, Mathf.Lerp(startLen, endLen, ang1 / (Mathf.PI * 0.5f)));
						newUV[uvIndex++] = new Vector2(0f, Mathf.Lerp(startLen, endLen, ang1 / (Mathf.PI * 0.5f)));	
					}
				}
				
				
				for (int j = 0; j < numTrisInCorner * 3; ++j){
					newTriangles[triIndex++] = firstJoinVertexIndex++;
				}
				
			}
			startLen = endLen;
				
			
			
		}
		
		// Ensure all z values are the same as that of the WireLine itself
		for (int i = 0; i < numVertices; ++i){
			Vector3 newPos = new Vector3(newVertices[i].x, newVertices[i].y, 0);
			newVertices[i] = newPos;
		}
		
		
	}
	
	void AssignMeshVars(){
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		
		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.triangles = newTriangles;
		
		mesh.UploadMeshData(false);
	}
	
	void SetupMaterials(){
		GetComponent<MeshRenderer>().materials[0].SetFloat("_IntensityWire", wireIntensity);
		GetComponent<MeshRenderer>().materials[0].SetColor("_ColorWire", wireColor);
		GetComponent<MeshRenderer>().materials[0].SetFloat("_Speed", speed);
		GetComponent<MeshRenderer>().materials[0].SetFloat("_Offset", offset);
	}
	

}
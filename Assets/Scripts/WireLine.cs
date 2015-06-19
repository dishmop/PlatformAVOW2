using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WireLine : MonoBehaviour {

	public float width;
	
	public enum EndType{
		kContinue,
		kEnd
	}
	
	public EndType end0;
	public EndType end1;
	
	public Color 	caseColor;
	public float 	caseIntensity;
	public Color	wireColor;
	public float 	wireIntensity;
	
	
	Vector3[] points;
	
	
	// For internal storage of vertex data
	
	// Useful numbers
	int numCorePoints;
	int numCoreLines;
	int numJunctions;
	int numCaps;
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
	
	public bool IsPointInside(Vector3 point, out float distAlong){
	
		if (pointsDirtyFlag){
			ConstructMesh();
		}
	
		// Ever set of 4 vertices is an axis aligned quad
		// I.e. mega trivial to determin if we are inside
		distAlong = 0;
		
		for (int i = 0; i < numVertices; i += 4){
			Vector3 rootPos = transform.TransformPoint(newVertices[i]);
			Vector3 otherPos0 = transform.TransformPoint(newVertices[i+1]);
			Vector3 otherPos1 = transform.TransformPoint(newVertices[i+2]);
			
			float minX = Mathf.Min (rootPos.x,  Mathf.Min (otherPos0.x, otherPos1.x));
			float maxX = Mathf.Max (rootPos.x,  Mathf.Max (otherPos0.x, otherPos1.x));
			float minY = Mathf.Min (rootPos.y,  Mathf.Min (otherPos0.y, otherPos1.y));
			float maxY = Mathf.Max (rootPos.y,  Mathf.Max (otherPos0.y, otherPos1.y));
			
			if (point.x > minX && point.x < maxX && point.y > minY && point.y < maxY){
				// If a mid segment (rather than an end or a coner)
					if ((i/4) % 2 == 1){
					// If horizontal segment
					if (MathUtils.FP.Feq(rootPos.y, otherPos1.y)){
						distAlong += Mathf.Abs (point.x  - rootPos.x);
					}
					// If vertical segment
					else if (MathUtils.FP.Feq(rootPos.x, otherPos1.x)){
						distAlong += Mathf.Abs (point.y  - rootPos.y);
					}
					else{
						DebugUtils.Assert(false, "Not horizontal nor vertical");
					}
					    
				}
				else{
					distAlong += 0.5f * (otherPos1 - rootPos).magnitude;
					
				}
				return true;
			}
			else{
				distAlong += (otherPos1 - rootPos).magnitude;
				
			}
		}
		return false;
	
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
		// If no points then no simplification
		if (points.Length == 0) return;
		
		// Make a temporary list to store our simplified points and add the first one
		List<Vector3> tempList = new List<Vector3>();
		tempList.Add (points[0]);
		
		// Now only add a new point if the direction changes - otherwise replace the last entry
		int lastDir = Directions.kNull;
		for (int i = 0; i < points.Length - 1; ++i){
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
		points = tempList.ToArray();
	
	}
	
	void CalcArraySizes(){
		// Set up number of elements in each array for the mesh
		numCorePoints = points.Length;
		numCoreLines = numCorePoints - 1;
		numJunctions = numCoreLines - 1;
		numCaps = 2;
		
		// Four points for each line segment and then 4 points at the junction to complete the corner square
		numVertices = 4 * (numCaps + numCoreLines + numJunctions);
		numUVs = numVertices;
		numTris = 2 * (numCaps + numCoreLines + numJunctions);
		numTriIndices = 3 * numTris;
	}
	
	void ConstructArrays(){
		// Set up the arrays themselves
		newVertices = new Vector3[numVertices];
		newUV = new Vector2[numUVs];
		newTriangles = new int[numTriIndices];
		
	}
	
	void FillArrays(){
		int vertexIndex = 0;
		int uvIndex = 0;
		int triIndex = 0;
				
		for (int i = 0; i < points.Length-1; ++i){
		
			// End points of core line
			Vector3 point0 = points[i];
			Vector3 point1 = points[i + 1];
			
			Vector3 lenVec = 0.5f * width * (point1 - point0).normalized;
			
			// Adjust the end positions in byb half  awidth
			point0 += lenVec;
			point1 -= lenVec;
			
			// half-width Vector perpendicular to core line
			Vector3 sideVec =  0.5f * width * Vector3.Cross(point1 - point0, new Vector3(0, 0, 1)).normalized;
			
			// If this is the first point, then do an initial cap (of half a width in side)
			if (i == 0){
				int firstCapVertexIndex = vertexIndex;
				
				Vector3 phantomPoint = point0 - lenVec;
				
				// The quad
				newVertices[vertexIndex++] = phantomPoint - sideVec;
				newVertices[vertexIndex++] = phantomPoint + sideVec;
				newVertices[vertexIndex++] = point0 - sideVec;
				newVertices[vertexIndex++] = point0 + sideVec;
				
				// Just use the thin band accross the middle of the texture
				switch (end0){
					case EndType.kContinue:{
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						break;
					}
					case EndType.kEnd:{
						newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
					newUV[uvIndex++] = new Vector2(1, 0.75f);
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						break;
					}
				}

				
				// Tri 0
				newTriangles[triIndex++] = firstCapVertexIndex + 0;
				newTriangles[triIndex++] = firstCapVertexIndex + 3;
				newTriangles[triIndex++] = firstCapVertexIndex + 1;
				
				// Tri 1
				newTriangles[triIndex++] = firstCapVertexIndex + 0;
				newTriangles[triIndex++] = firstCapVertexIndex + 2;
				newTriangles[triIndex++] = firstCapVertexIndex + 3;
			}
			
			int firstVertexIndex = vertexIndex;
			
			// The quad
			newVertices[vertexIndex++] = point0 - sideVec;
			newVertices[vertexIndex++] = point0 + sideVec;
			newVertices[vertexIndex++] = point1 - sideVec;
			newVertices[vertexIndex++] = point1 + sideVec;
			
			// Just use the thin band accross the middle of the texture
			newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
			newUV[uvIndex++] = new Vector2(1, 0.5f);
			newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
			newUV[uvIndex++] = new Vector2(1, 0.5f);
			
			// Tri 0
			newTriangles[triIndex++] = firstVertexIndex + 0;
			newTriangles[triIndex++] = firstVertexIndex + 3;
			newTriangles[triIndex++] = firstVertexIndex + 1;
			
			// Tri 1
			newTriangles[triIndex++] = firstVertexIndex + 0;
			newTriangles[triIndex++] = firstVertexIndex + 2;
			newTriangles[triIndex++] = firstVertexIndex + 3;
			
			// If point1 is NOT the last one - then making a joining quad
			if (i+1 != points.Length-1){
				int firstJoinVertexIndex = vertexIndex;
				
				Vector3 phantomPoint = point1 + 2f * lenVec;
				
				// The quad
				newVertices[vertexIndex++] = point1 - sideVec;
				newVertices[vertexIndex++] = point1 + sideVec;
				newVertices[vertexIndex++] = phantomPoint - sideVec;
				newVertices[vertexIndex++] = phantomPoint + sideVec;
				
				// Figure out if we are turning clockwise of anticlockwise
				Vector3 point2 = points[i + 2];
				float turnRes = Vector3.Dot(point2 - point1, sideVec);
				// If going in the same direction as side
				if (turnRes > 0){
					newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
					newUV[uvIndex++] = new Vector2(1, 0.5f);
					newUV[uvIndex++] = new Vector2(0.5f, 0);
					newUV[uvIndex++] = new Vector2(1, 0);
				}
				else{
					newUV[uvIndex++] = new Vector2(1, 0.5f);
					newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
					newUV[uvIndex++] = new Vector2(1, 0);
					newUV[uvIndex++] = new Vector2(0.5f, 0);
				}
				
				
				
				// Tri 0
				newTriangles[triIndex++] = firstJoinVertexIndex + 0;
				newTriangles[triIndex++] = firstJoinVertexIndex + 3;
				newTriangles[triIndex++] = firstJoinVertexIndex + 1;
				
				// Tri 1
				newTriangles[triIndex++] = firstJoinVertexIndex + 0;
				newTriangles[triIndex++] = firstJoinVertexIndex + 2;
				newTriangles[triIndex++] = firstJoinVertexIndex + 3;
			}
			
			// If this is the last point, then do an initial cap (of half a width in side)
			if (i + 1 == points.Length - 1){
				int firstCapVertexIndex = vertexIndex;
				
				Vector3 phantomPoint = point1 + lenVec;
				
				// The quad
				newVertices[vertexIndex++] = point1 - sideVec;
				newVertices[vertexIndex++] = point1 + sideVec;
				newVertices[vertexIndex++] = phantomPoint - sideVec;
				newVertices[vertexIndex++] = phantomPoint + sideVec;
				
				// Just use the thin band accross the middle of the texture
				switch (end1){
					case EndType.kContinue:{
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						break;
					}
					case EndType.kEnd:{
						newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
						newUV[uvIndex++] = new Vector2(1, 0.5f);
						newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
						newUV[uvIndex++] = new Vector2(1, 0.75f);
						break;
					}
				}
				
				
				// Tri 0
				newTriangles[triIndex++] = firstCapVertexIndex + 0;
				newTriangles[triIndex++] = firstCapVertexIndex + 3;
				newTriangles[triIndex++] = firstCapVertexIndex + 1;
				
				// Tri 1
				newTriangles[triIndex++] = firstCapVertexIndex + 0;
				newTriangles[triIndex++] = firstCapVertexIndex + 2;
				newTriangles[triIndex++] = firstCapVertexIndex + 3;
			}
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
		GetComponent<MeshRenderer>().materials[0].SetFloat("_IntensityCase", caseIntensity);
		GetComponent<MeshRenderer>().materials[0].SetColor("_ColorWire", wireColor);
		GetComponent<MeshRenderer>().materials[0].SetColor("_ColorCase", caseColor);
	}
	

}
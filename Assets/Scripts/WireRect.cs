using UnityEngine;
using System.Collections;

public class WireRect : MonoBehaviour {
	
	public Vector3 bottomLeft;
	public Vector3 topRight;
	
	
	public Color 	caseColor;
	public float 	caseIntensity;
	public Color	wireColor;
	public float 	wireIntensity;
	
	
	// For internal storage of vertex data
	
	// Useful numbers
	int numVertices;
	int numUVs;
	int numTris;
	int numTriIndices;
	
	// Internal storage of mesh data
	Vector3[] newVertices;
	Vector2[] newUV;
	int[] newTriangles;
	
	
	
	public void ConstructMesh(){
		CalcArraySizes();
		ConstructArrays();
		FillArrays();
		AssignMeshVars();
		SetupMaterials();
		
	}
	
	// Use this for initialization
	void Start () {
		ConstructMesh();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
	void CalcArraySizes(){

		// Four points for each line segment and then 4 points at the junction to complete the corner square
		numVertices = 9;
		numUVs = numVertices;
		numTris = 8;
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
		
		for (int i = 0; i < 3; ++i){
			float y = bottomLeft.y + (topRight.y - bottomLeft.y) * i / 2f;
			for (int j = 0; j < 3; ++j){
				float x = bottomLeft.x + (topRight.x - bottomLeft.x) * j / 2f;
				newVertices[vertexIndex++] = new Vector3(x, y, 0);
			}
		}
		// Bottom row
		newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
		newUV[uvIndex++] = new Vector2(0.75f, 0.75f);
		newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
		
		// Middle row
		newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
		newUV[uvIndex++] = new Vector2(0.75f, 0.5f);
		newUV[uvIndex++] = new Vector2(0.5f, 0.5f);
		
		// Top row
		newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
		newUV[uvIndex++] = new Vector2(0.75f, 0.75f);
		newUV[uvIndex++] = new Vector2(0.5f, 0.75f);
		
		
		// Tri 0
		newTriangles[triIndex++] = 0;
		newTriangles[triIndex++] = 4;
		newTriangles[triIndex++] = 1;
		
		// Tri 1
		newTriangles[triIndex++] = 0;
		newTriangles[triIndex++] = 3;
		newTriangles[triIndex++] = 4;
		
		// Tri 2
		newTriangles[triIndex++] = 1;
		newTriangles[triIndex++] = 5;
		newTriangles[triIndex++] = 2;
		
		// Tri 3
		newTriangles[triIndex++] = 1;
		newTriangles[triIndex++] = 4;
		newTriangles[triIndex++] = 5;
		
		// Tri 4
		newTriangles[triIndex++] = 3;
		newTriangles[triIndex++] = 7;
		newTriangles[triIndex++] = 4;
		
		// Tri 5
		newTriangles[triIndex++] = 3;
		newTriangles[triIndex++] = 6;
		newTriangles[triIndex++] = 7;	
		
		// Tri 6
		newTriangles[triIndex++] = 4;
		newTriangles[triIndex++] = 8;
		newTriangles[triIndex++] = 5;
		
		// Tri 7
		newTriangles[triIndex++] = 4;
		newTriangles[triIndex++] = 7;
		newTriangles[triIndex++] = 8;	
	}
	
	void AssignMeshVars(){
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.triangles = newTriangles;
		
		mesh.UploadMeshData(true);
	}
	
	void SetupMaterials(){
		GetComponent<MeshRenderer>().materials[0].color = caseColor;
		GetComponent<MeshRenderer>().materials[1].color = wireColor;
	}
	
}
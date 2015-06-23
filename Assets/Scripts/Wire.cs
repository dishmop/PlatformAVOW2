using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wire : MonoBehaviour {

	[System.Serializable]
	public struct EndData{
		public GameObject component;
		public Vector3 pos;
		public int dir;
		
	};
	
	public bool autoUpdateWires = true;
	
	public EndData[] ends = new EndData[2];
	
	
	public GameObject 	  	currentWire;
	
	public List<GameObject> junctions;
	
	
	List<Vector3>[] paths = new List<Vector3>[2];
	float			pathLength;
	
	// Debug (should be local)
	float distAlong;
	
	
	void ConstructMesh(){
		SetupEnds();
		SetupPath();
		currentWire = ConstructCentralWire();
	
	}
	
	public bool IsPointInside(Vector3 point, out float distAlong){
		return currentWire.GetComponent<WireLine>().IsPointInside(point, out distAlong);
	}
	
	
	public void ClearMesh(){
		foreach(Transform child in transform){
			Destroy(child.gameObject);
		}
	}
	
	public void SwapEnds(){
		EndData temp = ends[0];
		ends[0] = ends[1];
		ends[1] = temp;
		foreach (GameObject junction in junctions){
			junction.GetComponent<WireJunction>().propAlongWire = 1 - junction.GetComponent<WireJunction>().propAlongWire;
		}
	}
	
	
	
	// Given a proportion of the distance along the wire (from end0 to end1) and the 
	// direction we would like to attach from, this returns the position and the direction we will attach from
	// I.e. if we can attach from the right, we return 1
	public void CalcInfoFromProp(float prop, Vector3 otherPos, int otherIndex, out Vector3 pos, out int dir){
	
		DebugUtils.Assert(prop >= 0 && prop <= 1, "Prop passed in not between 0 and 1");
		float targetDist = prop * pathLength;
		
		// Figure out which path segment we are in and how much more "length" we need to travel
		int startIndex = -1;
		float remainingLengthToTarget = targetDist;
		float distTraveledSoFar = 0;
		
		float propAlongFinalSegment = -1;
		
		for (int i = 0; i < paths[0].Count-1; ++i){
			Vector3 segment = paths[0][i] - paths[0][i+1];
			float thisSegLen = segment.magnitude;
			if (thisSegLen > remainingLengthToTarget){
				startIndex = i;
				propAlongFinalSegment = remainingLengthToTarget / thisSegLen;
				break;
			}
			else{
				remainingLengthToTarget -= thisSegLen;
				distTraveledSoFar += thisSegLen;
			}
		}
		
		DebugUtils.Assert (startIndex >=0 && startIndex < paths[0].Count, "Trying to access wire segment outside of bounds");
		
		
		Vector3 localPos = Vector3.Lerp(paths[0][startIndex], paths[0][startIndex+1], propAlongFinalSegment);
		
		pos = transform.TransformPoint (localPos);
		
		// Figure out Direction
		Vector3 pathDir = paths[0][startIndex+1] - paths[0][startIndex];
		
		Vector3 desAttachDir = otherPos - pos;
		
		// If verical
		if (MathUtils.FP.Feq(pathDir.x, 0)){
			dir = (desAttachDir.x > 0) ? 1 : 3;
		}
		// If horizontal
		else if (MathUtils.FP.Feq (pathDir.y, 0)){
			dir = (desAttachDir.y > 0) ? 0 : 2;
		}
		else{
			dir = 4;
		}
		
	
	}
	
	
	void SetupEnds(){
		// if we are attached to a component then copy the various connection info from it
		for (int i = 0; i < 2; ++i){
			if (ends[i].component != null){
				ElectricalComponent connections = ends[i].component.GetComponent<ElectricalComponent>();
				connections.GetConnectionData(gameObject, out ends[i].dir, out ends[i].pos);
			}
		}
	}
	
	
	// Sets up the master path in paths[0]
	void SetupPath(){
		int[] workingDirs = new int[2];
		
		for (int i = 0; i < 2; ++i){
			workingDirs[i] = ends[i].dir;
			paths[i] = new List<Vector3>();
			paths[i].Add(ends[i].pos);
		}

		// We need to move away from where we want to get to, move away by one unit and then
		// change direction so we are now moving towards where we want to go
		for (int i = 0; i < 2; ++i){
			Vector3 startPosLoc = paths[i][0];
			
			Vector3 newPos = startPosLoc + Directions.GetDirVec(workingDirs[i]) * GameConfig.singleton.routingFirstStepDist;
			paths[i].Add (newPos);
		}
		
		// Set up the directions
		for (int i = 0; i < 2; ++i){
			Vector3 startPosLoc = paths[i][paths[i].Count-1];
			Vector3 endPosLoc = paths[1-i][paths[1-i].Count-1];
			
			workingDirs[i] = Directions.GetDirectionTowards(startPosLoc, endPosLoc, Directions.CalcOppDir(workingDirs[i]));
			
			//}
		}
		
		// Test that we are now heading towards each other
//		for (int i = 0; i < 2; ++i){
//			Vector3 startPosLoc = paths[i][paths[i].Count-1];
//			Vector3 endPosLoc = paths[1-i][paths[1-i].Count-1];
//			
//			DebugUtils.Assert(Directions.IsInSameDirection(startPosLoc, endPosLoc, workingDirs[i]), "Path ends not heading towards eachother");
//		}
//		
		Vector3 startPos = paths[0][paths[0].Count-1];
		Vector3 endPos = paths[1][paths[1].Count-1];
		
 		Vector3 centrePos = 0.5f * (startPos + endPos);

		// We are now down to two cases - either the two directions are opposite to one another
		// in which case we do an S shape
		// Or they are at right angles to one another - in which case we do an L shape
		if (workingDirs[0] == Directions.CalcOppDir(workingDirs[1])){
			// Make an S shape
			
			// If vertical
			if (workingDirs[0] == Directions.kUp || workingDirs[0] == Directions.kDown){
				paths[0].Add(new Vector3(startPos.x, centrePos.y, 0));
				paths[0].Add(new Vector3(endPos.x, centrePos.y, 0));
			}
			// If horizontal
			else{
				paths[0].Add(new Vector3(centrePos.x, startPos.y, 0));
				paths[0].Add(new Vector3(centrePos.x, endPos.y, 0));
			}
			
		}
		else{
			// Make an L shape
			
			// If vertical
			if (workingDirs[0] == Directions.kUp || workingDirs[0] == Directions.kDown){
				paths[0].Add(new Vector3(startPos.x, endPos.y, 0));
			}
			// If horizontal
			else{
				paths[0].Add(new Vector3(endPos.x, startPos.y, 0));
			}			
		}
		
		// Add in the points from path[1] to path[0]
		for (int i = paths[1].Count - 1; i >=0 ; --i){
			paths[0].Add (paths[1][i]);
		}
		
		// No use for the info in paths[1] anymore
		paths[1].Clear();
		
		// Make the z value 0
		for (int i = 0; i < paths[0].Count; ++i){
			Vector3 newPos = new Vector3(paths[0][i].x, paths[0][i].y, 0);
			paths[0][i] = newPos;
		}
		//centrePos.z = 0;
		
		
		transform.position = new Vector3(centrePos.x, centrePos.y, -2);
		
		// Offset the path so the object can be positioned at its centre
		for (int i = 0; i < paths[0].Count; ++i){
			paths[0][i] -= centrePos;
		}
		
		// Figure out the length of the path
		pathLength = 0;
		for (int i = 0; i < paths[0].Count-1; ++i){
			Vector3 segment = paths[0][i] - paths[0][i+1];
			pathLength += segment.magnitude;
		}
		
		
		
		
	
	}
	

	
	GameObject ConstructCentralWire(){
		GameObject wireLine =  GameObject.Instantiate(Factory.singleton.wireLinePrefab);
		wireLine.transform.SetParent(transform);
		wireLine.transform.localPosition = Vector3.zero;
		
		WireLine line = wireLine.GetComponent<WireLine>();
		
		line.SetNewPoints(paths[0].ToArray());
		line.end0 = WireLine.EndType.kContinue;
		line.end1 = WireLine.EndType.kContinue;
//		line.ConstructMesh();
		return wireLine;
	}
	
	void UpdateCentralWire(){
		currentWire.GetComponent<WireLine>().SetNewPoints(paths[0].ToArray());
	
	}
	

	// Use this for initialization
	void Start () {
		ConstructMesh();
	
	}
	

	
	public void HandleMouseInput(){
		// If this is the wire that is attaced to the cursor, then do nothing.
		if (ends[1].component != null && ends[1].component.GetComponent<ElectricalComponent>().type == ElectricalComponent.Type.kCursor){
			return;
		}
		// if this is the wire that is the parent of the junction at the cursor then do nothing
//		if (UI.singleton.cursorJunction != null){
//			if (UI.singleton.cursorJunction.GetComponent<WireJunction>().parentWire == gameObject){
//				return;
//			}
//		}
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition);
		distAlong = 0;
		if (IsPointInside(mouseWorldPos, out distAlong)){
			currentWire.GetComponent<WireLine>().wireIntensity = 2;
			UI.singleton.RegisterWireSelect(gameObject, distAlong / pathLength);
			
		}
		else{
			currentWire.GetComponent<WireLine>().wireIntensity = 1;
			UI.singleton.UnregisterWireSelect(gameObject);
		}
		
	}
	
	void Update(){
		HandleMouseInput();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		SetupEnds();
		SetupPath();
		UpdateCentralWire();
			
//		if (autoUpdateWires){
//			ClearMesh();
//			ConstructMesh();
//		}
	
	}
	
	void OnGUI(){
//		if (pathLength > 1){
//			GUI.Label(new Rect(0,0,Screen.width,Screen.height), "pathLength: " + pathLength + ", distAlong: " + distAlong);
//		}
	}
}

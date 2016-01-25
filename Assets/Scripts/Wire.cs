﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Wire : MonoBehaviour {

	[System.Serializable]
	public struct EndData{
		public GameObject component;
		public Vector3 pos;
		public int dir;
		
	};
	
	public bool markedForDeletion = false;
	public bool autoUpdateWires = true;
	public bool enableHandleInput = true;
	public int 	simEdgeId = -1;
	
	public EndData[] ends = new EndData[2];
	
	
	public GameObject 	  	currentWire;
	public float resistance = 0;
	
	public List<GameObject> junctions;
	
	
	List<Vector3>[] rawPaths = new List<Vector3>[2];
	
	// These split up the path into sections (with a different section between each junction
	List<List<Vector3>> usePaths = new List<List<Vector3>>();
	
	float			pathLength;
	
	// Debug (should be local)
	float distAlong;
	
	public float GetSimFwCurrent(){
		if (simEdgeId >= 0){
			return CircuitSimulator.singleton.GetEdge(simEdgeId).resFwCurrent;
		}
		return 0;
	}	
	
	
	void ReconstructUsePaths(){
		// How meny sections should we have
		int desNumSections = junctions.Count() + 1;
		if (desNumSections != usePaths.Count()){
			usePaths = new List<List<Vector3>>();
			for (int i = 0; i < desNumSections; ++i){
				usePaths.Add (new List<Vector3>());
			}
		}
		
		// Get an ordered list of the junctions in the wire
		List<GameObject> orderedJunctions = junctions.OrderBy(obj => obj.GetComponent<WireJunction>().propAlongWire).ToList();
		
		int segmentIndex = 0;
		int lastIndex = 0;
		float distTravelled = 0;
		Vector3 lastPos = rawPaths[0][lastIndex];
		
		// We need at least one path and the first point must go in it
		usePaths[segmentIndex] = new List<Vector3>();
		usePaths[segmentIndex].Add (lastPos);
		
		// If there are junctions, then do each segment at a time
		for  (int i = 0; i < orderedJunctions.Count(); ++i){
		
			// Distance that the junction is along the wire
			float juncDist = pathLength * orderedJunctions[i].GetComponent<WireJunction>().propAlongWire;

			// position of next point in the line path			
			Vector3 nextPos = rawPaths[0][lastIndex + 1];
			Vector3 pathSegment= nextPos - lastPos;
			float pathSegLength = pathSegment.magnitude;
			
			// If the junction is between this path segment then make a new path point and close off this usePath
			if (distTravelled + pathSegLength > juncDist){
				float distDownThisPathSegment = juncDist - distTravelled;
				float propDownThisPathSegment = distDownThisPathSegment / pathSegLength;
				Vector3 newPoint = Vector3.Lerp (lastPos, nextPos, propDownThisPathSegment);
				usePaths[segmentIndex].Add (newPoint);
				distTravelled = juncDist;
			}
			
		}
		
	}
	
	
	void ConstructMesh(){
		SetupEnds();
		SetupPath();
		currentWire = ConstructCentralWire();
	
	}
	
	public bool IsPointInside(Vector3 point, out float distAlong){
		return currentWire.GetComponent<WireLine>().IsPointInside(point, out distAlong);
	}
	
	
	public float CalMinDistToWire(Vector3 pos, out Vector3 nearstPos){
		return currentWire.GetComponent<WireLine>().CalMinDistToWire(pos, out nearstPos);
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
		UI.singleton.ValidateAttachedWire();
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
		
		for (int i = 0; i < rawPaths[0].Count-1; ++i){
			Vector3 segment = rawPaths[0][i] - rawPaths[0][i+1];
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
		
		DebugUtils.Assert (startIndex >=0 && startIndex < rawPaths[0].Count, "Trying to access wire segment outside of bounds");
		
		
		Vector3 localPos = Vector3.Lerp(rawPaths[0][startIndex], rawPaths[0][startIndex+1], propAlongFinalSegment);
		
		pos = transform.TransformPoint (localPos);
		
		// Figure out Direction
		Vector3 pathDir = rawPaths[0][startIndex+1] - rawPaths[0][startIndex];
		
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
		UI.singleton.ValidateAttachedWire();
	
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
			rawPaths[i] = new List<Vector3>();
			rawPaths[i].Add(ends[i].pos);
		}

		// We need to move away from where we want to get to, move away by one unit and then
		// change direction so we are now moving towards where we want to go
		for (int i = 0; i < 2; ++i){
			Vector3 startPosLoc = rawPaths[i][0];
			
			Vector3 newPos = startPosLoc + Directions.GetDirVec(workingDirs[i]) * GameConfig.singleton.routingFirstStepDist;
			rawPaths[i].Add (newPos);
		}
		
		// Set up the directions
		for (int i = 0; i < 2; ++i){
			Vector3 startPosLoc = rawPaths[i][rawPaths[i].Count-1];
			Vector3 endPosLoc = rawPaths[1-i][rawPaths[1-i].Count-1];
			
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
		Vector3 startPos = rawPaths[0][rawPaths[0].Count-1];
		Vector3 endPos = rawPaths[1][rawPaths[1].Count-1];
		
 		Vector3 centrePos = 0.5f * (startPos + endPos);

		// We are now down to two cases - either the two directions are opposite to one another
		// in which case we do an S shape
		// Or they are at right angles to one another - in which case we do an L shape
		if (workingDirs[0] == Directions.CalcOppDir(workingDirs[1])){
			// Make an S shape
			
			// If vertical
			if (workingDirs[0] == Directions.kUp || workingDirs[0] == Directions.kDown){
				rawPaths[0].Add(new Vector3(startPos.x, centrePos.y, 0));
				rawPaths[0].Add(new Vector3(endPos.x, centrePos.y, 0));
			}
			// If horizontal
			else{
				rawPaths[0].Add(new Vector3(centrePos.x, startPos.y, 0));
				rawPaths[0].Add(new Vector3(centrePos.x, endPos.y, 0));
			}
			
		}
		else{
			// Make an L shape
			
			// If vertical
			if (workingDirs[0] == Directions.kUp || workingDirs[0] == Directions.kDown){
				rawPaths[0].Add(new Vector3(startPos.x, endPos.y, 0));
			}
			// If horizontal
			else{
				rawPaths[0].Add(new Vector3(endPos.x, startPos.y, 0));
			}			
		}
		
		// Add in the points from path[1] to path[0]
		for (int i = rawPaths[1].Count - 1; i >=0 ; --i){
			rawPaths[0].Add (rawPaths[1][i]);
		}
		
		// No use for the info in paths[1] anymore
		rawPaths[1].Clear();
		
		// Make the z value 0
		for (int i = 0; i < rawPaths[0].Count; ++i){
			Vector3 newPos = new Vector3(rawPaths[0][i].x, rawPaths[0][i].y, 0);
			rawPaths[0][i] = newPos;
		}
		//centrePos.z = 0;
		
		
		transform.position = new Vector3(centrePos.x, centrePos.y, -2);
		
		// Offset the path so the object can be positioned at its centre
		for (int i = 0; i < rawPaths[0].Count; ++i){
			rawPaths[0][i] -= centrePos;
		}
		
		// Figure out the length of the path
		pathLength = 0;
		for (int i = 0; i < rawPaths[0].Count-1; ++i){
			Vector3 segment = rawPaths[0][i] - rawPaths[0][i+1];
			pathLength += segment.magnitude;
		}
		
		
		
		
	
	}
	

	
	GameObject ConstructCentralWire(){
		GameObject wireLine =  GameObject.Instantiate(Factory.singleton.wireLinePrefab);
		wireLine.transform.SetParent(transform);
		wireLine.transform.localPosition = Vector3.zero;
		
		WireLine line = wireLine.GetComponent<WireLine>();
		
		line.SetNewPoints(rawPaths[0].ToArray());
		line.end0 = WireLine.EndType.kContinue;
		line.end1 = WireLine.EndType.kContinue;
//		line.ConstructMesh();
		return wireLine;
	}
	
	void UpdateCentralWire(){
		currentWire.GetComponent<WireLine>().SetNewPoints(rawPaths[0].ToArray());
	
	}
	

	// Use this for initialization
	void Start () {
		ConstructMesh();
	
	}
	

	
	public void HandleMouseInput(){
		if (!enableHandleInput) return;
		
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
			//currentWire.GetComponent<WireLine>().wireIntensity = 2;
			float distUse = Mathf.Min (Mathf.Max (distAlong, 0.5f), pathLength-0.5f);
			UI.singleton.RegisterWireSelect(gameObject, distUse / pathLength);
			
		}
		else{
			//	currentWire.GetComponent<WireLine>().wireIntensity = 1;
			UI.singleton.UnregisterWireSelect(gameObject);
		}
		UI.singleton.ValidateAttachedWire();
		
	}
	
	void Update(){
		HandleMouseInput();
		if (!MathUtils.FP.Feq(resistance, 0)){
			GetComponent<AudioSource>().volume = 0.2f;
		}
		else{
			GetComponent<AudioSource>().volume = 0f;
		}
	}
	
	// Get the voltage at one end of the wire and apply it to the colour
	void SetupColors(){
		
		ElectricalComponent component = ends[0].component.GetComponent<ElectricalComponent>();
		if (component.simNodeIndices != null && component.simNodeIndices.Count() != 0){
			int index = component.GetConnectionDataIndex(gameObject);
			if (index == -1){
				// Something's gone wrong 
				return;
			}
			int simNodeIndex = component.simNodeIndices[index];
			if (simNodeIndex >= 0){
				float voltage = CircuitSimulator.singleton.allNodes[simNodeIndex].resVoltage;
				Color wireCol = Color.Lerp (GameConfig.singleton.lowVolt, GameConfig.singleton.highVolt, voltage);
				currentWire.GetComponent<WireLine>().wireColor = wireCol;
			}
			if (simEdgeId >= 0){
				currentWire.GetComponent<WireLine>().SetSpeed(GetSimFwCurrent());
			}
		}
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		SetupEnds();
		SetupPath();
		UpdateCentralWire();
		resistance = Mathf.Max (0, resistance - 0.1f);
			

		SetupColors();
		UI.singleton.ValidateAttachedWire();
	
	}
	
	void OnGUI(){
//		if (pathLength > 1){
//			GUI.Label(new Rect(0,0,Screen.width,Screen.height), "pathLength: " + pathLength + ", distAlong: " + distAlong);
//		}
	}
}

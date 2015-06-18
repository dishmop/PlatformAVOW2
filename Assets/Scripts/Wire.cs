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
	
	List<Vector3>[] paths = new List<Vector3>[2];
	
	
	GameObject 	  	currentWire;
	
	
	public void ConstructMesh(){
		SetupEnds();
		SetupPath();
		currentWire = ConstructCentralWire();
	
	}
	
	
	public void ClearMesh(){
		foreach(Transform child in transform){
			Destroy(child.gameObject);
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

		// If we need to move away from where we want to get to, move away by one unit and then
		// change direction so we are now moving towards where we want to go
		for (int i = 0; i < 2; ++i){
			Vector3 startPosLoc = paths[i][0];
			Vector3 endPosLoc = paths[1-i][0];
			
			if (!Directions.IsInSameDirection(startPosLoc, endPosLoc, workingDirs[i])){
				Vector3 newPos = startPosLoc + Directions.GetDirVec(workingDirs[i]) * GameConfig.singleton.routingFirstStepDist;
				workingDirs[i] = Directions.GetDirectionTowards(startPosLoc, endPosLoc, Directions.CalcOppDir(workingDirs[i]));
				paths[i].Add (newPos);

			}
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
		centrePos.z = 0;
		
		
		// Offset the path so the object can be positioned at its centre
		transform.position = new Vector3(centrePos.x, centrePos.y, transform.position.z);
		for (int i = 0; i < paths[0].Count; ++i){
			paths[0][i] -= centrePos;
		}
		
		
	
	}
	

	
	GameObject ConstructCentralWire(){
		GameObject wire =  GameObject.Instantiate(Factory.singleton.wireLinePrefab);
		wire.transform.SetParent(transform);
		wire.transform.localPosition = Vector3.zero;
		
		WireLine line = wire.GetComponent<WireLine>();
		
		line.points = paths[0].ToArray();
		line.end0 = WireLine.EndType.kEnd;
		line.end1 = WireLine.EndType.kEnd;
		line.ConstructMesh();
		return wire;
	}

	// Use this for initialization
	void Start () {
		ConstructMesh();
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (autoUpdateWires){
			ClearMesh();
			ConstructMesh();
		}
	
	}
}

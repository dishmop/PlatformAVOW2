using UnityEngine;
using System.Collections;

public class UpdateManager : MonoBehaviour {
	public static UpdateManager singleton = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Circuit.singleton.GameUpdate();
		CircuitSimulator.singleton.GameUpdate();
		AVOWSim.singleton.GameUpdate();
		
//		Debug.Log ("-----");
		//		foreach (var node in allNodes){
		//			Debug.Log("   [" + node.GetID() + " - " + node.debugName + "] = " + node.resVoltage + ", clique = " + node.isInBatteryClique);
		//		}
		
//		foreach (var edge in CircuitSimulator.singleton.allEdges){
//			if (!edge.isInBatteryClique) continue;
//			Debug.Log("   [" + edge.GetID() + " - (" + edge.nodes[0].debugName + ", " + edge.nodes[1].debugName + ") ]: h0 = " + edge.h0 + ", hWidth = " + edge.hWidth + ", v0 = " + edge.nodes[0].resVoltage + ", v1 = " + edge.nodes[1].resVoltage);
//		}
		
	
	
	}
	
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
}

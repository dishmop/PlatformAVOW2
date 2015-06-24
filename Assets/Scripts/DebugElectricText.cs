using UnityEngine;
using System.Collections;

public class DebugElectricText : MonoBehaviour {

	public GameObject electricsGO;

	// Use this for initialization
	void Start () {
	
	}
	
	void Update(){
		transform.GetChild(0).gameObject.SetActive(GameConfig.singleton.showDebug);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		ElectricalComponent component = electricsGO.GetComponent<ElectricalComponent>();
		CircuitSimulator sim = CircuitSimulator.singleton;
		if (component.simEdgeIndex >= 0 && GameConfig.singleton.showDebug){
			float current = sim.allEdges[component.simEdgeIndex].resFwCurrent;
			
			float volt0 = sim.allNodes[component.simNodeIndices[0]].resVoltage;
			float volt1 = sim.allNodes[component.simNodeIndices[1]].resVoltage;
			
			GetComponent<TextMesh>().text = "C=" + current + ",V0=" + volt0 + ",V1=" + volt1;
		}
		else{
			GetComponent<TextMesh>().text = "";
		}
	
		
	
	}
}

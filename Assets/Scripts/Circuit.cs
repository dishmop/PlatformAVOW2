using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Circuit : MonoBehaviour {

	public static Circuit singleton = null;
	List<GameObject> electricalComponents = new List<GameObject>();
	List<GameObject> wires = new List<GameObject>();

	public void RegisterComponent(GameObject electricalComponent){
		electricalComponents.Add(electricalComponent);
	}
	
	
	public GameObject GetElectricalComponent(int index){
		return electricalComponents[index];
	}
	
	
	public void AddConnection(GameObject component0, int connector0, GameObject component1, int connector1){
		GameObject newWire = GameObject.Instantiate(Factory.singleton.wire);
		newWire.transform.SetParent(transform);
		newWire.GetComponent<Wire>().ends[0].component = component0;
		newWire.GetComponent<Wire>().ends[1].component = component1;
		
		component0.GetComponent<ElectricalComponent>().connectionData[connector0].wire = newWire;
		component1.GetComponent<ElectricalComponent>().connectionData[connector1].wire = newWire;
		
		
	}
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}

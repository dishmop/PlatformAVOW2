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
	
	public void AddWire(GameObject wire){
		wire.transform.SetParent(transform);
		wires.Add (wire);
	}
	
	public void RemoveWire(GameObject wire){
		int index = wires.IndexOf(wire);
		wires.RemoveAt(index);
//		ElectricalComponent component0 = wire.GetComponent<Wire>().ends[0].component.GetComponent<ElectricalComponent>();
//		ElectricalComponent component1 = wire.GetComponent<Wire>().ends[1].component.GetComponent<ElectricalComponent>();
//		
//		int index0 = component0.GetConnectionDataIndex(wire);
//		int index1 = component1.GetConnectionDataIndex(wire);
//		
//		component0.ClearConnectionData(index0);
//		component1.ClearConnectionData(index1);
//		
//		Destroy(wire);
	}
	
	
	
	public void AddConnection(GameObject component0, int connector0, GameObject component1, int connector1){
		GameObject newWire = GameObject.Instantiate(Factory.singleton.wirePrefab);
		newWire.transform.SetParent(transform);
		newWire.GetComponent<Wire>().ends[0].component = component0;
		newWire.GetComponent<Wire>().ends[1].component = component1;
		
		component0.GetComponent<ElectricalComponent>().connectionData[connector0].wire = newWire;
		component1.GetComponent<ElectricalComponent>().connectionData[connector1].wire = newWire;
		
		
	}
	
	
	public void RemoveJunctionsJoining(GameObject wire){
		List<GameObject> removalWires = new List<GameObject>();
		List<int> removalIndices = new List<int>();
		foreach (GameObject testWire in wires){
			for (int i = 0; i < testWire.GetComponent<Wire>().junctions.Count; ++i){
				GameObject junction = testWire.GetComponent<Wire>().junctions[i];
				if (junction.GetComponent<ElectricalComponent>().connectionData[0].wire == wire){
					//int index = testWire.GetComponent<Wire>().junctions.FindIndex(junction);
					//testWire.GetComponent<Wire>().junctions.Remove(junction);
					removalWires.Add (testWire);
					removalIndices.Add (i);
					Destroy (junction);
				}
			}
		}
		for (int i = 0; i < removalWires.Count; ++i){
			removalWires[i].GetComponent<Wire>().junctions.RemoveAt(removalIndices[i]);
		}
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

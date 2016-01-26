using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Circuit : MonoBehaviour {

	public static Circuit singleton = null;
	public List<GameObject> electricalComponentGOs = new List<GameObject>();
	public List<GameObject> wireGOs = new List<GameObject>();
	
	class Spark{
		public GameObject[] electralCompoentGOs = new GameObject[2];
		public int[] indices = new int[2];
		public float resistance = 0;
	}
	Spark spark;
	
	CircuitSimulator sim = null;

	public void EnableSpark(GameObject obj0, int index0, GameObject obj1, int index1, float resistance){
		if (spark == null){
			spark = new Spark();
		}
		spark.electralCompoentGOs[0] = obj0;
		spark.electralCompoentGOs[1] = obj1;
		spark.indices[0] = index0;
		spark.indices[1] = index1;
		spark.resistance = resistance;
	}
	
	public void DisableSpark(){
		spark = null;
	}
	
	public void RegisterComponent(GameObject electricalComponent){
		electricalComponentGOs.Add(electricalComponent);
	}
	
	public void UnregisterComponent(GameObject electricalComponent){
		electricalComponentGOs.Remove(electricalComponent);
	}
	
	public void RemoveAllWires(){
		for (int i = 0 ; i < wireGOs.Count(); ++i){
			RemoveJunctionsJoining(wireGOs[i]);
			Destroy (wireGOs[i]);
		}
		wireGOs = new List<GameObject>();
		
		// Ensure all compoents have no connections
		foreach (GameObject componentGOs in electricalComponentGOs){
			ElectricalComponent component = componentGOs.GetComponent<ElectricalComponent>();
			for (int i = 0; i < component.connectionData.Count(); ++i){
				
				component.connectionData[i].wire = null;
			}
		}
	}
	
	
	public GameObject GetElectricalComponent(int index){
		return electricalComponentGOs[index];
	}
	
	public void AddWire(GameObject wire){
		wire.transform.SetParent(transform);
		wireGOs.Add (wire);
	}
	
	public void RemoveWire(GameObject wire){
		int index = wireGOs.IndexOf(wire);
		wireGOs.RemoveAt(index);
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
	
		if (wire == null) return;

		// Find wires that contain a child junction matching the end of this wire.	
		foreach (GameObject testWire in wireGOs){
			for (int i = 0; i < testWire.GetComponent<Wire>().junctions.Count; ++i){
				GameObject junction = testWire.GetComponent<Wire>().junctions[i];
				if (junction.GetComponent<ElectricalComponent>().connectionData[0].wire == wire){
					//int index = testWire.GetComponent<Wire>().junctions.FindIndex(junction);
					//testWire.GetComponent<Wire>().junctions.Remove(junction);
					testWire.GetComponent<Wire>().junctions.RemoveAt(i);
					Destroy (junction);
					break;
				}
			}
		}
		
		// We also need to remove any wires which are connected to child junctions on THIS wire. 
		List<GameObject> removalWires = new List<GameObject>();
		foreach (GameObject junctionGO in wire.GetComponent<Wire>().junctions){
			WireJunction junction = junctionGO.GetComponent<WireJunction>();
			removalWires.Add (junction.GetOtherWire());
			// First remove them from the circuit (so we don't try and remove them again
			wireGOs.Remove(junction.GetOtherWire());
		}
		
		// Now we can go about actually deleting them (and recursively deleting their junctions too
		foreach (GameObject wireGO in removalWires){
			RemoveJunctionsJoining(wireGO);
			Destroy (wireGO);
			
		}
		
		
	}
	
	void AddSparkToSim(Spark thisSpark){
		// Error catching:
		if (thisSpark.indices[0] < 0 || thisSpark.indices[0] >= thisSpark.electralCompoentGOs[0].GetComponent<ElectricalComponent>().simNodeIndices.Length){
			Debug.Log("Indexing Error thisSpark.indices[0]: " + thisSpark.indices[0]);
			return;
		}
		if (thisSpark.indices[1] < 0 || thisSpark.indices[1] >= thisSpark.electralCompoentGOs[1].GetComponent<ElectricalComponent>().simNodeIndices.Length){
			Debug.Log("Indexing Error thisSpark.indices[1]: " + thisSpark.indices[1]);
			return;
		}
		int simIndex0 = thisSpark.electralCompoentGOs[0].GetComponent<ElectricalComponent>().simNodeIndices[thisSpark.indices[0]];
		int simIndex1 = thisSpark.electralCompoentGOs[1].GetComponent<ElectricalComponent>().simNodeIndices[thisSpark.indices[1]];
		float hOrder = (thisSpark.electralCompoentGOs[0].transform.position.x + thisSpark.electralCompoentGOs[1].transform.position.x) * 0.5f;
		sim.AddLoadEdge(simIndex0, simIndex1, thisSpark.resistance, hOrder, CircuitSimulator.EdgeType.kBlue);
	}
	
	
	public void AddWireToSim(GameObject wireGO){
	
		UI.singleton.ValidateAttachedWire();
		
	
		Wire wire = wireGO.GetComponent<Wire>();
//		
//		if (wire == null || wire.junctions == null){
//			Debug.Log ("Error wire or wire.junction is null");
//		}
//		
//		foreach (GameObject go in wire.junctions){
//			if (go == null){
//				Debug.Log ("Error go is null");
//			}
//			
//		}
//		
//		if (wire.simEdgeId == 8){
//			for (int i = 0 ; i < wire.junctions.Count(); ++i){
//				Debug.Log ("[" + i + "] - Junction: " + ((wire.junctions[i] == null) ? "null" : wire.junctions[i].GetInstanceID().ToString()));
//			}
//		}
		
		
		
		// Get an ordered list of the junctions in the wire
		List<GameObject> thisJunctions = wire.junctions.OrderBy(obj => obj.GetComponent<WireJunction>().propAlongWire).ToList();
		
		ElectricalComponent lastComponent = wire.ends[0].component.GetComponent<ElectricalComponent>();
		int lastIndex = lastComponent.GetConnectionDataIndex(wireGO);
		if (lastIndex == -1){
			// something's gone  a bit wrong
			return;
		}
		
		for (int i = 0; i < thisJunctions.Count(); ++i){
			ElectricalComponent nextComponent = thisJunctions[i].GetComponent<ElectricalComponent>();
			int nextIndex = 0;	// always 0 for junctions
			
			
			// Add the edge to the circuit sim
			if (MathUtils.FP.Feq(wire.resistance, 0)){
				nextComponent.simEdgeId = sim.AddConductorEdge(lastComponent.simNodeIndices[lastIndex], nextComponent.simNodeIndices[nextIndex]);
			}
			else{
				nextComponent.simEdgeId = sim.AddLoadEdge(lastComponent.simNodeIndices[lastIndex], nextComponent.simNodeIndices[nextIndex], wire.resistance, wire.ends[0].component.transform.position.x, CircuitSimulator.EdgeType.kBlue);
			}
			
			// Set last to next
			lastComponent = nextComponent;
			lastIndex = nextIndex;
			
		}
		UI.singleton.ValidateAttachedWire();
		
		if (wire.ends[1].component == null){
			Debug.Log("!!!");
			return;
		}
		
		ElectricalComponent endComponent = wire.ends[1].component.GetComponent<ElectricalComponent>();
		int endIndex = endComponent.GetConnectionDataIndex(wireGO);
		
		// Add the final segment in
//		Debug.Log("lastIndex = " + lastIndex + ", endIndex = " + endIndex);

		if (MathUtils.FP.Feq(wire.resistance, 0)){
			wire.simEdgeId = sim.AddConductorEdge(lastComponent.simNodeIndices[lastIndex], endComponent.simNodeIndices[endIndex]);
		}
		else{
			wire.simEdgeId = sim.AddLoadEdge(lastComponent.simNodeIndices[lastIndex], endComponent.simNodeIndices[endIndex], wire.resistance, wire.ends[0].component.transform.position.x, CircuitSimulator.EdgeType.kBlue);
		}
		
		
		
	}
	
	void AddComponentToSim(GameObject componentGO){
		
		ElectricalComponent component = componentGO.GetComponent<ElectricalComponent>();
				
		for (int i = 0; i < component.simNodeIndices.Count (); ++i){
			component.simNodeIndices[i] = sim.AddNode(component.name + "_" + i);
		}
		if (component.type == ElectricalComponent.Type.kLoad){
			component.simEdgeId = sim.AddLoadEdge(component.simNodeIndices[0], component.simNodeIndices[1], component.resistance, component.gameObject.transform.position.x);
		}
		if (component.type == ElectricalComponent.Type.kVoltageSource){
			component.simEdgeId = sim.AddVoltageSourceEdge(component.simNodeIndices[0], component.simNodeIndices[1], component.voltageRise, component.resistance);
		}
		
		// Do any internal routing
		foreach (ElectricalComponent.InternalRoute route in component.internalRouting){
			sim.AddLoadEdge(component.simNodeIndices[route.connectionIndex0], component.simNodeIndices[route.connectionIndex1], route.resistance, component.gameObject.transform.position.x);
		}
	}
	
	// Initialise the circuit simulator and run it
	public void GameUpdate(){
		UI.singleton.ValidateAttachedWire();
		ClearCircuitSimData();
		
		
		sim.ClearCircuit();
		
		// Add all voltage sources - need to do this first due to way loops are found
		// assumes our first node is attached to the circuit
//		Debug.Log ("---");
		foreach (GameObject componentGO in electricalComponentGOs){
			ElectricalComponent component = componentGO.GetComponent<ElectricalComponent>();
			if (component.type == ElectricalComponent.Type.kVoltageSource){
				AddComponentToSim(componentGO);

			}

		}
		
		// Add all other nodes
		foreach (GameObject componentGO in electricalComponentGOs){
			ElectricalComponent component = componentGO.GetComponent<ElectricalComponent>();
			if (component.type != ElectricalComponent.Type.kVoltageSource){
				AddComponentToSim(componentGO);
				
			}
		}
		// Add the UI's junction
//		if (UI.singleton.cursorJunction != null){	
//			AddComponentToSim(UI.singleton.cursorJunction );
//		}
		
		// Add all the wires in as 0 resistance edges. In the case of a wire having several junctions on it
		// We add an "edge" for each segment of wire
		foreach (GameObject wireGO in wireGOs){
			AddWireToSim(wireGO);
			
		}
		// Add the UI's wire
		if (UI.singleton.attachedWire != null){	
			AddWireToSim(UI.singleton.attachedWire );
		}
		
		if (spark != null){
			AddSparkToSim(spark);
		}
		

		
		
	}
	
	void ClearCircuitSimData(){
		foreach (GameObject componentGO in electricalComponentGOs){
			componentGO.GetComponent<ElectricalComponent>().ClearSimData();
		}
	}
	
		
	void Start(){
		sim = CircuitSimulator.singleton;
		
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

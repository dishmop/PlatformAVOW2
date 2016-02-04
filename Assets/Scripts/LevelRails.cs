using UnityEngine;
using System.Collections;
using System.Linq;

public class LevelRails : MonoBehaviour {

	public GameObject connectionTLGO;
	public GameObject connectionTRGO;
	public GameObject connectionBLGO;
	public GameObject connectionBRGO;
	public GameObject cellComponentGO;
	

	GameObject wireTGO;
	GameObject wireBGO;
	
	GameObject junctionT;
	GameObject junctionB;
	
	
	bool firstUpdate = true;
	

		
	// Use his for initialization
	void Start () {
		wireTGO = SetupWire(connectionTLGO, 0, connectionTRGO, 0);
		wireBGO = SetupWire(connectionBLGO, 0, connectionBRGO, 0);
		
		junctionT = ConstructJunction(wireTGO, cellComponentGO, 1);
		SetupWire(cellComponentGO, 1 , junctionT, 0);
		
		junctionB = ConstructJunction(wireBGO, cellComponentGO, 0);
		SetupWire(cellComponentGO, 0 , junctionB, 0);

	}
	
	GameObject ConstructJunction(GameObject wireGO, GameObject otherComponentGO, int index){
	
		GameObject newJunction = GameObject.Instantiate(Factory.singleton.wireJunctionPrefab);
		
		newJunction.transform.SetParent(transform);
		newJunction.GetComponent<WireJunction>().parentWire = wireGO;
		newJunction.GetComponent<WireJunction>().AddSelfToParent();
		
		
		newJunction.GetComponent<WireJunction>().propAlongWire = GetPropAlong(otherComponentGO, index, wireGO);
		newJunction.GetComponent<WireJunction>().otherComponent = otherComponentGO;
		newJunction.GetComponent<WireJunction>().otherComponentIndex = index;
		newJunction.GetComponent<ElectricalComponent>().connectionData[0].isInteractive = false;
		
		
		return newJunction;
	}
	
	
	GameObject SetupWire(GameObject connection0, int index0, GameObject connection1, int index1){
		GameObject newWire = GameObject.Instantiate(Factory.singleton.wirePrefab);
		newWire.transform.SetParent(transform);
		newWire.GetComponent<Wire>().ends[0].component = connection0;
		newWire.GetComponent<Wire>().ends[1].component = connection1;
		
		connection0.GetComponent<ElectricalComponent>().connectionData[index0].wire = newWire;
		connection1.GetComponent<ElectricalComponent>().connectionData[index1].wire = newWire;
		
		Circuit.singleton.AddWire(newWire);
		return newWire;
	}
	
	float GetPropAlong(GameObject component, int index, GameObject wire){
		// This assumes that the wire is horizontal
		// Get the x posiiton of the component connector
		Vector3 connectionPos = component.transform.TransformPoint(component.GetComponent<ElectricalComponent>().connectionData[index].pos);
		float xPos = connectionPos.x;
		
		// Get the two ends of the wire
		Vector3 startPos = wire.GetComponent<Wire>().ends[0].pos;
		Vector3 endPos = wire.GetComponent<Wire>().ends[1].pos;
		Vector3 startToEnd = endPos - startPos;
		return (xPos - startPos.x) / (startToEnd.x);
	}
	
	
	
	
//	void  UpdateEmptyConnectors(){
//		int index = isOn ? 1 : 2;
//		
//		connectionGO1.transform.position =  transform.TransformPoint(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].pos);
//		connectionGO1.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.CalcOppDir(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].dir);
//		
//		
//		if (switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices != null && switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices.Count() != 0){
//			connectionGO0.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[0];
//			connectionGO1.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[index];
//		}
//		
//		
//	}
	
	
	// Update is called once per frame
	void Update () {
	
		// Needs to be done after Start() has been called on the junctions
		if (firstUpdate){
			firstUpdate = false;
		}
//		UpdateEmptyConnectors();
//		for (int i = 0; i < 2; ++i){		
//			connectionGO[i*2+0].GetComponent<ElectricalComponent>().simNodeIndices [0] = levelElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[i*2+0];
//			connectionGO[i*2+1].GetComponent<ElectricalComponent>().simNodeIndices [0] = levelElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[i*2+1];
//		}
		
		
	}
	
}

using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
	public static UI singleton = null;
	
	public GameObject cursorJunction;
	public GameObject attachedWire;
	public Transform cursorTransform;
	
	Vector3 mouseWorldPos;
	GameObject selectedComponent = null;
	int selectedConnectorIndex = -1;

	
	public void RegisterSelected(GameObject electricalComponentGO, int index){
		ValidateAttachedWire();
	
		if (attachedWire != null){
			// If we are already attached to this, then ignore
			ElectricalComponent component = electricalComponentGO.GetComponent<ElectricalComponent>();
			Wire wire = attachedWire.GetComponent<Wire>();
			if (component.connectionData[index].wire == attachedWire && 
			    wire.ends[0].component == electricalComponentGO){
				selectedComponent = null;
				selectedConnectorIndex = -1;
				ValidateAttachedWire();
				
				return;
			}
			// If there is already another wire attached to this then ignore
			if (electricalComponentGO.GetComponent<ElectricalComponent>().connectionData[index].wire != null && 
			electricalComponentGO.GetComponent<ElectricalComponent>().connectionData[index].wire != attachedWire){
				selectedComponent = null;
				selectedConnectorIndex = -1;
				ValidateAttachedWire();
				return;
			}
		}
		
		selectedComponent = electricalComponentGO;
		selectedConnectorIndex = index;
		
		// Check if we have selected another connector, and if so snap the wire to it
		if (attachedWire != null){

			attachedWire.GetComponent<Wire>().ends[1].component = selectedComponent;
			cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = null;
			selectedComponent.GetComponent<ElectricalComponent>().connectionData[selectedConnectorIndex].wire = attachedWire;
			
		}
		ValidateAttachedWire();
	}
	
	public void RegisterWireSelect(GameObject wire, float propAlong){
//		Debug.Log(Time.fixedTime + ": RegisterWireSelect()");
		ValidateAttachedWire();

		if ( attachedWire == null) return;
		
		if (attachedWire == wire) return;
		
		if (cursorJunction == null){
			cursorJunction = GameObject.Instantiate(Factory.singleton.wireJunctionPrefab);
			cursorJunction.transform.SetParent(transform);
			cursorJunction.GetComponent<WireJunction>().parentWire = wire;
			cursorJunction.GetComponent<WireJunction>().AddSelfToParent();
		}
		
		cursorJunction.GetComponent<WireJunction>().propAlongWire = propAlong;	
		cursorJunction.GetComponent<WireJunction>().otherComponent = attachedWire.GetComponent<Wire>().ends[0].component;
		cursorJunction.GetComponent<WireJunction>().otherComponentIndex = attachedWire.GetComponent<Wire>().ends[0].component.GetComponent<ElectricalComponent>().GetConnectionDataIndex(attachedWire);
		
		
		
		attachedWire.GetComponent<Wire>().ends[1].component = cursorJunction;
		cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = null;
		cursorJunction.GetComponent<ElectricalComponent>().connectionData[0].wire = attachedWire;
		ValidateAttachedWire();
	}

	
	public void UnregisterWireSelect(GameObject wire){
//		Debug.Log(Time.fixedTime + ": UnregisterWireSelect()");
		WireJunction junction = (cursorJunction != null) ? cursorJunction.GetComponent<WireJunction>() : null;
		if (attachedWire != null && cursorJunction != null && junction.parentWire == wire){
		
			attachedWire.GetComponent<Wire>().ends[1].component = cursorTransform.gameObject;
			cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = attachedWire;
			cursorJunction.GetComponent<WireJunction>().RemoveSelfFromParent();
			Destroy (cursorJunction);
		}
		
		ValidateAttachedWire();
	
	}


	public void UnregisterSelected(GameObject electricalComponent, int index){
		if (selectedComponent == electricalComponent && selectedConnectorIndex == index){
			
			// Check if we have a wire attached and is so move it back on to the cursor
			if (attachedWire != null){
				
				attachedWire.GetComponent<Wire>().ends[1].component = cursorTransform.gameObject;
				cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = attachedWire;
				selectedComponent.GetComponent<ElectricalComponent>().connectionData[selectedConnectorIndex].wire = null;
				
			}
			
			selectedComponent = null;
			selectedConnectorIndex = -1;
		}
		ValidateAttachedWire();
	}
	

	public void AttachConnector(GameObject electricalComponent, int connectionIndex){
		DebugUtils.Assert(attachedWire == null, "Wire already attached");
		
		attachedWire = GameObject.Instantiate(Factory.singleton.wirePrefab);
		attachedWire.transform.SetParent(transform);
		attachedWire.GetComponent<Wire>().ends[0].component = electricalComponent;
		attachedWire.GetComponent<Wire>().ends[1].component = cursorTransform.gameObject;
		
		electricalComponent.GetComponent<ElectricalComponent>().connectionData[connectionIndex].wire = attachedWire;
		cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = attachedWire;
		ValidateAttachedWire();
	}
	
	public void ReleaseConnector(){
		if (attachedWire != null){
			Circuit.singleton.RemoveJunctionsJoining(attachedWire);
			Destroy (attachedWire);
		}
		ValidateAttachedWire();
		
	}
	
	public void TransferWire(GameObject wire){
		attachedWire = wire;
		ValidateAttachedWire();
	}
	
	public void ValidateAttachedWire(){
		if (attachedWire != null){
			Wire thisWire = attachedWire.GetComponent<Wire>();
			DebugUtils.Assert (thisWire.ends[0].component != null, "thisWire.ends[0].component != null");
			DebugUtils.Assert (thisWire.ends[1].component != null, "thisWire.ends[1].component != null");
			
			DebugUtils.Assert (thisWire.ends[0].component.GetComponent<ElectricalComponent>().GetConnectionDataIndex(attachedWire) != -1, "thisWire.ends[0].component.GetComponent<ElectricalComponent>().GetConnectionDataIndex(attachedWire) != -1");
			DebugUtils.Assert (thisWire.ends[1].component.GetComponent<ElectricalComponent>().GetConnectionDataIndex(attachedWire) != -1, "thisWire.ends[1].component.GetComponent<ElectricalComponent>().GetConnectionDataIndex(attachedWire) != -1");;
			
		}
		
	}

	
	// Use this for initialization
	void Start () {
		cursorTransform = transform.FindChild("Cursor");
	
	}
	
	// Update is called once per frame
	void Update () {
	
//		Debug.Log(Time.fixedTime + ": Update()");
		
	
		// Calc the mouse posiiton on world space
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition);
		mouseWorldPos.z = cursorTransform.position.z;
		cursorTransform.position = mouseWorldPos;
		
		
		if (Input.GetMouseButtonUp(0)){
			if (attachedWire != null && (selectedComponent != null || cursorJunction != null)){
//				attachedWire.GetComponent<Wire>().currentWire.GetComponent<WireLine>().caseColor = Color.black;
				Circuit.singleton.AddWire(attachedWire);
				
				if (cursorJunction != null){
				
//					cursorJunction.GetComponent<WireJunction>().AddSelfToParent();
					cursorJunction = null;
				
				}
				
				attachedWire = null;
				

			
			}
			UI.singleton.ReleaseConnector();

		}
		
			
		// Figure out the direction of the cursor connection
		if (attachedWire != null){
			Vector3 thisEnd = cursorTransform.position;
			
			Wire.EndData otherEnd = attachedWire.GetComponent<Wire>().ends[0];
			Vector3 otherEndPos = otherEnd.pos + Directions.GetDirVec(otherEnd.dir) * GameConfig.singleton.routingFirstStepDist;
			
			cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.GetDominantDirection(otherEndPos - thisEnd);
		}
		
		if (Input.GetMouseButtonUp(0)){
			ReleaseConnector();
		}
		
//		if (attachedWire != null){
//			GameObject wireLine = attachedWire.GetComponent<Wire>().currentWire;
//			if (wireLine != null){
//				wireLine.GetComponent<WireLine>().caseColor = GameConfig.singleton.selectedColor;
//			}
//		}
		

		ValidateAttachedWire();
	
	}
	
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
	
	void OnGUI(){
//		if (attachedWire != null){
//			Wire wire = attachedWire.GetComponent<Wire>();
//			ElectricalComponent component = wire.ends[1].component.GetComponent<ElectricalComponent>();
//			GUI.Label(new Rect(0,0,Screen.width,Screen.height), "attachedWire.end[1].simNodeID = " + component.simNodeIndices[component.GetConnectionDataIndex(attachedWire)]);
//			
//		}
	}

	
}

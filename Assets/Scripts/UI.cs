using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
	public static UI singleton = null;
	
	Vector3 mouseWorldPos;
	Transform cursorTransform;
	GameObject attachedWire;
	GameObject selectedComponent = null;
	int selectedConnectorIndex = -1;
	
	public void RegisterSelected(GameObject electricalComponent, int index){
	
		if (attachedWire != null){
			// If we are already attached to this, then ignore
			if (electricalComponent.GetComponent<ElectricalComponent>().connectionData[index].wire == attachedWire && 
			    attachedWire.GetComponent<Wire>().ends[0].component == electricalComponent){
				selectedComponent = null;
				selectedConnectorIndex = -1;
				
				return;
			}
			// If there is already another wire attached to this then ignore
			if (electricalComponent.GetComponent<ElectricalComponent>().connectionData[index].wire != null && 
			electricalComponent.GetComponent<ElectricalComponent>().connectionData[index].wire != attachedWire){
				selectedComponent = null;
				selectedConnectorIndex = -1;
				return;
			}
		}
		
	
				
		
		selectedComponent = electricalComponent;
		selectedConnectorIndex = index;
		
		// Check if we have selected another connector, and if so snap the wire to it
		if (attachedWire != null){

			attachedWire.GetComponent<Wire>().ends[1].component = selectedComponent;
			cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = null;
			selectedComponent.GetComponent<ElectricalComponent>().connectionData[selectedConnectorIndex].wire = attachedWire;
			
		}
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
	}
	

	public void AttachConnector(GameObject electricalComponent, int connectionIndex){
		DebugUtils.Assert(attachedWire == null, "Wire already attached");
		
		attachedWire = GameObject.Instantiate(Factory.singleton.wirePrefab);
		attachedWire.transform.SetParent(transform);
		attachedWire.GetComponent<Wire>().ends[0].component = electricalComponent;
		attachedWire.GetComponent<Wire>().ends[1].component = cursorTransform.gameObject;
		
		electricalComponent.GetComponent<ElectricalComponent>().connectionData[connectionIndex].wire = attachedWire;
		cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = attachedWire;
	}
	
	public void ReleaseConnector(){
		Destroy (attachedWire);
		
	}

	
	// Use this for initialization
	void Start () {
		cursorTransform = transform.FindChild("Cursor");
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// Calc the mouse posiiton on world space
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition);
		mouseWorldPos.z = cursorTransform.position.z;
		cursorTransform.position = mouseWorldPos;
		
		
		if (Input.GetMouseButtonUp(0)){
			if (attachedWire != null && selectedComponent != null){
				attachedWire.GetComponent<Wire>().currentWire.GetComponent<WireLine>().caseColor = Color.black;
				Circuit.singleton.AddWire(attachedWire);
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
		
		if (attachedWire != null){
			GameObject wireLine = attachedWire.GetComponent<Wire>().currentWire;
			if (wireLine != null){
				wireLine.GetComponent<WireLine>().caseColor = GameConfig.singleton.selectedColor;
			}
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

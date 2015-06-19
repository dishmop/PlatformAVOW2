using UnityEngine;
using System.Collections;

public class ElectricalComponent : MonoBehaviour {


	public enum Type{
		kUnknown,
		kVoltageSource,
		kLoad,
		kCursor,
		kJunction
	};
	
	public Type type = Type.kUnknown;
	
	public bool active = true;

	[System.Serializable]
	public struct ConnectionData{
		public GameObject wire;
		public Vector3 pos;
		public int dir;
		public GameObject emptyConnector;
		public bool uiIsSelected;
		public bool uiIsAttached;
		
	};
	
	public ConnectionData[] connectionData;
	
	
	public void GetConnectionData(GameObject wire, out int dir, out Vector3 pos){
		foreach (ConnectionData data in connectionData){
			if (data.wire == wire){
				dir = data.dir;
				pos = transform.TransformPoint(data.pos);
				return;
			}
		}
		dir = Directions.kNull;
		pos = Vector3.zero;
		
	}
	
	public int GetConnectionDataIndex(GameObject wire){
		for (int i = 0; i < connectionData.Length; ++i){
		
			if (connectionData[i].wire == wire){
				return i;
			}
		}
		return -1;
	}
	
	public void ClearConnectionData(int index){
		connectionData[index].wire = null;
		connectionData[index].uiIsSelected = false;
		connectionData[index].uiIsAttached = false;
		connectionData[index].emptyConnector.SetActive(true);
		
	}
	
	
	void SetupConnectorPositions(){
		for (int i = 0; i < connectionData.Length; ++i){
			Vector3 wirePos = new Vector3(connectionData[i].pos.x, connectionData[i].pos.y, -2);
			connectionData[i].emptyConnector.transform.localPosition = wirePos;
			WireLine wireLine = connectionData[i].emptyConnector.GetComponent<WireLine>();
			
			Vector3 goDir = Directions.GetDirVec(connectionData[i].dir);
			
			wireLine.points = new Vector3[2];
			wireLine.points[0] = Vector3.zero;
			wireLine.points[1] = goDir * wireLine.width * 1.01f;
			wireLine.end0 = WireLine.EndType.kContinue;
			wireLine.end1 = WireLine.EndType.kEnd;
			
		}
	}
	
	
	void Start(){
		Circuit.singleton.RegisterComponent(gameObject);

		
		// Set up the little bits of wire that are the conneciton points
		for (int i = 0; i < connectionData.Length; ++i){
			connectionData[i].emptyConnector = GameObject.Instantiate(Factory.singleton.wireLinePrefab);
			connectionData[i].emptyConnector.transform.parent = transform;
		}
		SetupConnectorPositions();
		

		
	}
	
	void Update(){

		// Set the connectors to invisible if a wire is attached
		foreach (ConnectionData data in connectionData){
			data.emptyConnector.SetActive(data.wire == null);
		}
		if (type != Type.kCursor){
			HandleMouseInput();
		}
		for (int i = 0; i < connectionData.Length; ++i){
			connectionData[i].emptyConnector.SetActive(active && connectionData[i].wire == null);
		}
		SetupConnectorPositions();
	}
	
	
	
	
	void HandleMouseInput(){
	
		// Calc the mouse posiiton on world space
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition);
		
		if (!GameMode.singleton.isEditingCircuit){
			for (int i = 0; i < connectionData.Length; ++i){
				connectionData[i].uiIsSelected = false;
				connectionData[i].uiIsAttached = false;
				UI.singleton.ReleaseConnector();
			}
		}
		else{
			
			// Test if inside any of the connectors
			for (int i = 0; i < connectionData.Length; ++i){
				WireLine wireLine = connectionData[i].emptyConnector.GetComponent<WireLine>();
				float halfWidth =  wireLine.width * 0.501f;
				Vector3 minPos = transform.TransformPoint(connectionData[i].pos) - new Vector3(halfWidth, halfWidth, 0) + Directions.GetDirVec(connectionData[i].dir) * halfWidth;
				Vector3 maxPos = transform.TransformPoint(connectionData[i].pos) + new Vector3(halfWidth, halfWidth, 0) + Directions.GetDirVec(connectionData[i].dir) * halfWidth;
				
				connectionData[i].uiIsSelected = (mouseWorldPos.x > minPos.x && mouseWorldPos.x < maxPos.x && mouseWorldPos.y > minPos.y && mouseWorldPos.y < maxPos.y);
				if (connectionData[i].uiIsSelected){
					UI.singleton.RegisterSelected(gameObject, i);
				}
				else{
					UI.singleton.UnregisterSelected(gameObject, i);
				}
			}
			
			for (int i = 0; i < connectionData.Length; ++i){
				if (connectionData[i].uiIsSelected && Input.GetMouseButtonDown(0)){
					if (connectionData[i].wire == null){
						connectionData[i].uiIsAttached = true;
						UI.singleton.AttachConnector(gameObject, i);
					}
					else{
						// Get the wire
						//Wire thisWire = connectionData[i].wire.GetComponent<Wire>();
						GameObject comp0 = connectionData[i].wire.GetComponent<Wire>().ends[0].component;
						int index0 = comp0.GetComponent<ElectricalComponent>().GetConnectionDataIndex(connectionData[i].wire);
						
						GameObject comp1 = connectionData[i].wire.GetComponent<Wire>().ends[1].component;
						int index1 = comp1.GetComponent<ElectricalComponent>().GetConnectionDataIndex(connectionData[i].wire);
						
						GameObject otherComp = null;
						int otherIndex = -1;
						
						if (comp0 == gameObject && index0 == i){
							otherComp = comp1;
							otherIndex = index1;
							
						}
						else if (comp1 == gameObject && index1 == i){
							otherComp = comp0;
							otherIndex = index0;
						}
						else{
							DebugUtils.Assert(false, "Removing a wire which is not attached as we thought.");
						}
						
						Circuit.singleton.RemoveWire(connectionData[i].wire);
						
						connectionData[i].uiIsAttached = true;
						UI.singleton.AttachConnector(otherComp, otherIndex);
						UI.singleton.RegisterSelected(gameObject, i);
						
						
						
					}
					
				}
				if (Input.GetMouseButtonUp(0)){
					connectionData[i].uiIsAttached = false;
				}
			}
		}
		
		// Visualise UI stat
		foreach (ConnectionData data in connectionData){
		
			Color caseColor = Color.black;
			if (data.uiIsAttached){
				caseColor = Color.cyan;
			}
			else if (data.uiIsSelected){
				caseColor = Color.green;
			}
			
			
			data.emptyConnector.GetComponent<WireLine>().caseColor = caseColor;
		}
		
		
		// Set the cursor cubes position
		mouseWorldPos.z = transform.position.z;
	}
	

	

}

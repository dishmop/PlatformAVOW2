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
	}
	
	
	void SetupConnectorPositions(){
		for (int i = 0; i < connectionData.Length; ++i){
		
			if (connectionData[i].emptyConnector != null){
				Vector3 connectorPos = new Vector3(connectionData[i].pos.x, connectionData[i].pos.y, -2);
				connectionData[i].emptyConnector.transform.localPosition = connectorPos;
			}
			/*
			WireLine wireLine = connectionData[i].emptyConnector.GetComponent<WireLine>();
			
			Vector3 goDir = Directions.GetDirVec(connectionData[i].dir);
			
			Vector3[] newPoints = new Vector3[2];
			newPoints[0] = Vector3.zero;
			newPoints[1] = goDir * wireLine.width * 1.01f;
			wireLine.SetNewPoints(newPoints);
			wireLine.end0 = WireLine.EndType.kContinue;
			wireLine.end1 = WireLine.EndType.kEnd;
			*/
			
		}
	}
	
	
	void Start(){
		Circuit.singleton.RegisterComponent(gameObject);

		
		// Set up the little bits of wire that are the conneciton points
		if (type != Type.kCursor){
			if (type != Type.kJunction){
				for (int i = 0; i < connectionData.Length; ++i){
					connectionData[i].emptyConnector = GameObject.Instantiate(Factory.singleton.socketPrefab);
					connectionData[i].emptyConnector.transform.parent = transform;
				}
			}
			else{
				connectionData[0].emptyConnector = GameObject.Instantiate(Factory.singleton.socketTPrefab);
				connectionData[0].emptyConnector.transform.parent = transform;
			}
			SetupConnectorPositions();
		}
		

		
	}
	
	void Update(){


		if (type != Type.kCursor){
			HandleMouseInput();
		}
		for (int i = 0; i < connectionData.Length; ++i){
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
			
				if (connectionData[i].emptyConnector != null){
					Collider2D collider = connectionData[i].emptyConnector.GetComponent<Collider2D>();
					connectionData[i].uiIsSelected = collider.OverlapPoint(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
				}
			

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
						int otherWireIndex = -1;
						
						if (comp0 == gameObject && index0 == i){
							otherComp = comp1;
							otherIndex = index1;
							otherIndex = 1;
							
						}
						else if (comp1 == gameObject && index1 == i){
							otherComp = comp0;
							otherIndex = index0;
							otherIndex = 0;
						}
						else{
							DebugUtils.Assert(false, "Removing a wire which is not attached as we thought.");
						}
						
						// If we have grabbed the 0 end of the wire - swap it over
						if (otherIndex == 1){
							connectionData[i].wire.GetComponent<Wire>().SwapEnds();
							
						}
						
						Circuit.singleton.RemoveWire(connectionData[i].wire);
						UI.singleton.TransferWire(connectionData[i].wire);
						
						connectionData[i].uiIsAttached = true;
						//UI.singleton.AttachConnector(otherComp, otherIndex);
						UI.singleton.RegisterSelected(gameObject, i);
						

						
						
						if (type == Type.kJunction){
							GameObject parentWire = GetComponent<WireJunction>().parentWire;
							parentWire.GetComponent<Wire>().junctions.Remove (gameObject);
							parentWire.GetComponent<Wire>().HandleMouseInput();

							
							Destroy(gameObject);

						}
						
						
						
					}
					
				}
				if (Input.GetMouseButtonUp(0)){
					connectionData[i].uiIsAttached = false;
				}
			}
		}
		
		// Visualise UI state
		foreach (ConnectionData data in connectionData){
			data.emptyConnector.transform.GetChild(0).gameObject.SetActive(data.uiIsSelected);
		}
		
		
		// Set the cursor cubes position
		mouseWorldPos.z = transform.position.z;
	}
	

	

}

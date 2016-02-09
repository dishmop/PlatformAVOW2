using UnityEngine;
using System.Collections;
using System.Linq;

public class ElectricalComponent : MonoBehaviour {

	public enum Type{
		kUnknown,
		kVoltageSource,
		kLoad,
		kCursor,
		kSwitch,
		kJunction,
		kInternal,
		kEnd,
	};
	
	public Color squareCol;
	
	
	int rotDirAdd = 0;
	
	public Type type = Type.kUnknown;
	
	[System.Serializable]
	public class InternalRoute{
		public int connectionIndex0;
		public int connectionIndex1;
		public float resistance;
	}
	public InternalRoute[] internalRouting;
	public enum PolarityType{
		kAgnostic,
		kBidirectional,
//		kUnidirectional,
	}
	
	public PolarityType polarityType = PolarityType.kAgnostic;
	

	[System.Serializable]
	public struct ConnectionData{
		public GameObject wire;
		public Vector3 pos;
		public int dir;
		public GameObject emptyConnector;
		public bool uiIsSelected;
		public bool uiIsAttached;
		public bool isInteractive;
		
		
	};
	
	public ConnectionData[] connectionData;
	
	// Indicies into the circuit simulator objects
	// If we are a load or voltage source then we have two nodes
	// If we are a junction, then we only have one
	public int[] simNodeIndices;
	public int simEdgeId = -1;
	public float voltageRise = 0;
	public float resistance = 0;
	
	public float GetSimFwCurrent(){
		if (simEdgeId >= 0){
			return CircuitSimulator.singleton.GetEdge(simEdgeId).resFwCurrent;
		}
		return 0;
	}
	
	public float GetVoltageMin(){
		if (simEdgeId >= 0){
			return CircuitSimulator.singleton.GetEdge(simEdgeId).outNode.resVoltage;
		}
		return 0;
	}
	
	public float GetVoltageMax(){
		if (simEdgeId >= 0){
			return CircuitSimulator.singleton.GetEdge(simEdgeId).inNode.resVoltage;
		}
		return 0;
	}
	
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
	
	public void ClearSimData(){
		for (int i = 0; i < simNodeIndices.Count(); ++i){
			simNodeIndices[i] = -1;
		}
		simEdgeId = -1;
	}
	
	
	void SetupConnectorPositions(){
		for (int i = 0; i < connectionData.Length; ++i){
		
			if (connectionData[i].emptyConnector != null){
				Vector3 connectorPos = new Vector3(connectionData[i].pos.x, connectionData[i].pos.y, -2);
				connectionData[i].emptyConnector.transform.localPosition = connectorPos;
				if (type != Type.kJunction){
					int useDir = (connectionData[i].dir + Directions.kNumDirections - rotDirAdd) % Directions.kNumDirections;
					connectionData[i].emptyConnector.transform.localRotation = Quaternion.Euler(0, 0, useDir * 90);
				}
			}

			
		}
	}
	
	
	void SetupConnectorColours(){
		for (int i = 0; i < connectionData.Length; ++i){
			
			if (connectionData[i].emptyConnector != null){
				connectionData[i].emptyConnector.GetComponent<Renderer>().material.color = connectionData[i].isInteractive ? Color.white : new Color(0.25f, 0.25f, 0.25f, 1);
			}
		}
	}	
	
	void SetupSimIndices(){
		int numNodeIndices = 0;
		switch (type){
			case Type.kCursor:{
				numNodeIndices = 1;
				break;
			}
			case Type.kJunction:{
				numNodeIndices = 1;
				break;
			}
	
			case Type.kLoad:{
				numNodeIndices = 2;
				break;
			}
			case Type.kVoltageSource:{
				numNodeIndices = 2;
				break;
			}
			case Type.kSwitch:{
				numNodeIndices = 3;
				break;
			}
			case Type.kEnd:{
				numNodeIndices = 1;
				break;
			}
			case Type.kInternal:{
				numNodeIndices = 1;
				break;
			}
		}
		simNodeIndices = new int[numNodeIndices];
		
		
		ClearSimData();
	
	}
	
	
	void Start(){
	
		float angle = transform.rotation.eulerAngles.z;
		if (MathUtils.FP.Feq(angle, 0)){
			rotDirAdd = 0;
		}
		else if (MathUtils.FP.Feq(angle, 270)){
			rotDirAdd = 1;
		}
		else if (MathUtils.FP.Feq(angle, 180)){
			rotDirAdd = 2;
		}
		else if (MathUtils.FP.Feq(angle, 90)){
			rotDirAdd = 3;
		}
		
		for (int i = 0; i < connectionData.Length; ++i){
			connectionData[i].dir = (connectionData[i].dir + rotDirAdd) % Directions.kNumDirections;
		}
		
		if (type != Type.kInternal){
			Circuit.singleton.RegisterComponent(gameObject);
		}

		
		// Set up the little bits of wire that are the conneciton points
		if (type != Type.kCursor && type != Type.kInternal){
			if (type != Type.kJunction && type != Type.kEnd){
				for (int i = 0; i < connectionData.Length; ++i){
					connectionData[i].emptyConnector = GameObject.Instantiate(Factory.singleton.socketPrefab);
					connectionData[i].emptyConnector.transform.parent = transform;
				}
				if (type == Type.kLoad){
					switch (polarityType){
						case PolarityType.kAgnostic:
						{
							connectionData[0].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kNeutral;
							connectionData[1].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kNeutral;
							break;
						}
						case PolarityType.kBidirectional:
						{
							connectionData[0].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kPlus;
							connectionData[1].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kMinus;
							break;
						}
					}
				}
				if (type == Type.kVoltageSource){
					connectionData[0].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kMinus;
					connectionData[1].emptyConnector.GetComponent<PipeSocket>().type = PipeSocket.Type.kPlus;
				}
			}
			else if (type == Type.kJunction){
				connectionData[0].emptyConnector = GameObject.Instantiate(Factory.singleton.socketTPrefab);
				connectionData[0].emptyConnector.transform.parent = transform;
			}
		
			else if (type == Type.kEnd){
				for (int i = 0; i < connectionData.Length; ++i){
					connectionData[i].emptyConnector = GameObject.Instantiate(Factory.singleton.socketEndPrefab);
					connectionData[i].emptyConnector.transform.parent = transform;
				}
			}
			SetupConnectorPositions();
			SetupConnectorColours();
		}
		SetupSimIndices();
		for (int i = 0; i < connectionData.Length; ++i){
			GameObject connector = connectionData[i].emptyConnector;
			if (connector == null) continue;
			Collider2D collider = connector.GetComponent<Collider2D>();
			
			if (collider == null) continue;
			collider.enabled = connectionData[i].isInteractive;
			
		}
		if (!MathUtils.FP.Fleq(resistance, 0)){
			squareCol = GameConfig.singleton.GetNextSquareCol();
		}
	}
	
	void OnDestroy(){
		if (Circuit.singleton != null) Circuit.singleton.UnregisterComponent(gameObject);
	}
	
	void Update(){


		if (type != Type.kCursor && type != Type.kInternal){
			HandleMouseInput();
		}
		for (int i = 0; i < connectionData.Length; ++i){
		}
		SetupConnectorPositions();
		SetupConnectorColours();
		
		UI.singleton.ValidateAttachedWire();
	}
	
	void FixedUpdaste(){
		UI.singleton.ValidateAttachedWire();
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
			

				if (connectionData[i].uiIsSelected ){
					bool ok = UI.singleton.RegisterSelected(gameObject, i);
					if (!ok){
						connectionData[i].uiIsSelected = false;
						return;
					}
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
						
						int otherIndex = -1;
						
						if (comp0 == gameObject && index0 == i){
							otherIndex = index1;
							otherIndex = 1;
							
						}
						else if (comp1 == gameObject && index1 == i){
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
						bool ok = UI.singleton.RegisterSelected(gameObject, i);
						if (!ok){
							connectionData[i].uiIsSelected = false;
							return;
						}
						
						

						
						
						if (type == Type.kJunction){
							GameObject parentWire = GetComponent<WireJunction>().parentWire;
							
							parentWire.GetComponent<Wire>().RemoveJunction(gameObject);
							
							/// Attach the UI wire to the cursor
							UI.singleton.attachedWire.GetComponent<Wire>().ends[1].component = UI.singleton.cursorTransform.gameObject;
							UI.singleton.cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].wire = UI.singleton.attachedWire;
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
		
		UI.singleton.ValidateAttachedWire();
	}
	
	
	

	

}

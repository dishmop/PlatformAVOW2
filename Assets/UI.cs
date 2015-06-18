using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
	public static UI singleton = null;
	
	Vector3 mouseWorldPos;
	Transform cursorTransform;
	GameObject attachedWire;

	public void AttachConnector(GameObject electricalComponent, int connectionIndex){
		DebugUtils.Assert(attachedWire == null, "Wire already attached");
		
		attachedWire = GameObject.Instantiate(Factory.singleton.wire);
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
		
		// Figure out the direction of the cursor connection
		if (attachedWire != null){
			Wire.EndData otherEnd = attachedWire.GetComponent<Wire>().ends[0];
			Vector3 otherEndPos = otherEnd.pos + Directions.GetDirVec(otherEnd.dir) * GameConfig.singleton.routingFirstStepDist;
			
			cursorTransform.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.GetDominantDirection(otherEndPos - cursorTransform.position);
			
			Debug.DrawLine(cursorTransform.position, otherEndPos, Color.white);
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

using UnityEngine;
using System.Collections;

public class WireJunction : MonoBehaviour {

	public GameObject parentWire;
	public float propAlongWire = 0.5f;
	public GameObject otherComponent;
	public int otherComponentIndex;
	
	public void AddSelfToParent(){
	
		parentWire.GetComponent<Wire>().AddJunction(gameObject);
		transform.SetParent(parentWire.transform);
		UI.singleton.ValidateAttachedWire();
		
//		Debug.Log ("AddSelfToParent: " + gameObject.GetInstanceID());
		
		
	}
	
	public GameObject GetOtherWire(){
		return otherComponent.GetComponent<ElectricalComponent>().connectionData[otherComponentIndex].wire;
	}
	
	public void RemoveSelfFromParent(){
		
		parentWire.GetComponent<Wire>().RemoveJunction(gameObject);
		UI.singleton.ValidateAttachedWire();
		
	}

	// Use this for initialization
	void Start () {
	
	}

	
	// Update is called once per frame
	void Update () {
		// Find the position we should place ourselves
		Wire wire = parentWire.GetComponent<Wire>();
		Vector3 newPos;
		int newDir;
		
		ElectricalComponent electricalComponent = GetComponent<ElectricalComponent>();
		GetComponent<Renderer>().material.color = electricalComponent.connectionData[0].isInteractive ? Color.white : new Color(0.25f, 0.25f, 0.25f, 1);
		
		// Calc the position of the thing we are tryig to attach on
		if (otherComponent == null || otherComponentIndex == -1){
			return;
		}
		
		Vector3 basePos = otherComponent.transform.position;
		int otherWireDir = otherComponent.GetComponent<ElectricalComponent>().connectionData[otherComponentIndex].dir;
		
		
		wire.CalcInfoFromProp(propAlongWire, basePos + Directions.GetDirVec(otherWireDir) * GameConfig.singleton.routingFirstStepDist, otherWireDir, out newPos, out newDir);
		newPos.z = -3;
		transform.position = newPos;
		GetComponent<ElectricalComponent>().connectionData[0].dir = newDir;
		
		
		// Set the orientation to match where the empty pipe is
		switch (GetComponent<ElectricalComponent>().connectionData[0].dir){
			case Directions.kUp:{
				transform.rotation = Quaternion.Euler(0, 0, 180);
				break;
			}
			case Directions.kRight:{
				transform.rotation = Quaternion.Euler(0, 0, 90);
				break;
			}
			case Directions.kDown:{
				transform.rotation = Quaternion.Euler(0, 0, 0);
				break;
			}
			case Directions.kLeft:{
				transform.rotation = Quaternion.Euler(0, 0, 270);
				break;
			}
		}
		UI.singleton.ValidateAttachedWire();
		
	}
	
	void OnGUI(){
	//	GUI.Label(new Rect(0,0,Screen.width,Screen.height), "newDir: " + GetComponent<ElectricalComponent>().connectionData[0].dir);
	}
}

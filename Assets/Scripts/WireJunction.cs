using UnityEngine;
using System.Collections;

public class WireJunction : MonoBehaviour {

	public GameObject parentWire;
	public float propAlongWire = 0.5f;
	public GameObject otherComponent;
	
	public void AddSelfToParent(){
	
		parentWire.GetComponent<Wire>().junctions.Add(gameObject);
		transform.SetParent(parentWire.transform);
		
		
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
		wire.CalcInfoFromProp(propAlongWire, otherComponent.transform.position, out newPos, out newDir);
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
		
	}
	
	void OnGUI(){
		GUI.Label(new Rect(0,0,Screen.width,Screen.height), "newDir: " + GetComponent<ElectricalComponent>().connectionData[0].dir);
	}
}

using UnityEngine;
using System.Collections;

public class WireJunction : MonoBehaviour {

	public GameObject parentWire;
	public float propAlongWire = 0.5f;

	// Use this for initialization
	void Start () {
	
	}

	
	// Update is called once per frame
	void Update () {

		
		// Find the position we should place ourselves
		Wire wire = parentWire.GetComponent<Wire>();
		Vector3 newPos;
		int newDir;
		wire.CalcInfoFromProp(propAlongWire, new Vector3(-1, 0, 0), out newPos, out newDir);
		newPos.z = -3;
		transform.position = newPos;
	
	}
}

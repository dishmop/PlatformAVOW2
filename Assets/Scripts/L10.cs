using UnityEngine;
using System.Collections;

public class L10 : MonoBehaviour {

	public GameObject pistonGO;
	public GameObject text;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		bool isBackwards = pistonGO.GetComponent<ElectricalComponent>().GetSimFwCurrent() < 0;
		
		text.SetActive(isBackwards);
		
	
	}
}

using UnityEngine;
using System.Collections;

public class L03 : MonoBehaviour {

	public GameObject textMouseGO;
	public GameObject textFlowGO;
	public GameObject magnetElectricsGO;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		textMouseGO.SetActive(GameMode.singleton.isEditingCircuit);
		bool doorIsOpen = Mathf.Abs(magnetElectricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent()) > 0.9f;
		textFlowGO.SetActive(doorIsOpen);
	
	}
}

using UnityEngine;
using System.Collections;

public class L13 : MonoBehaviour {

	public GameObject piston1GO;
	public GameObject piston2GO;
	public GameObject text;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		bool isBothOn = 
			!MathUtils.FP.Feq (piston1GO.GetComponent<ElectricalComponent>().GetSimFwCurrent(), 0) &&
			!MathUtils.FP.Feq (piston2GO.GetComponent<ElectricalComponent>().GetSimFwCurrent(), 0);
		
		text.SetActive(isBothOn);
		
	
	}
}

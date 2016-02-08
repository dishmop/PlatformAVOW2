using UnityEngine;
using System.Collections;

public class L14 : MonoBehaviour {

	public GameObject pistonGO1;
	public GameObject pistonGO2;
	public GameObject textSuccess;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		
		bool isSuccess = 
			MathUtils.FP.Feq(Mathf.Abs(pistonGO1.GetComponent<ElectricalComponent>().GetSimFwCurrent()),  1f) && 
			MathUtils.FP.Feq(Mathf.Abs (pistonGO2.GetComponent<ElectricalComponent>().GetSimFwCurrent()),  1f);
			
		textSuccess.SetActive(isSuccess);
		
	}
}

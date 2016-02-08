using UnityEngine;
using System.Collections;

public class L12 : MonoBehaviour {

	public GameObject terminal1;
	public GameObject pistonGO1;
	public GameObject pistonGO2;
	public GameObject textParallel;
	public GameObject textBackwards;
	public GameObject textSuccess;

	public GameObject terminal2;
	public GameObject step2;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		bool isOnTerminal1 = terminal1.GetComponent<Terminal>().isEditing;
		bool isOnTerminal2 = terminal2.GetComponent<Terminal>().isEditing;
		
		
		bool isParallel = 
			(Mathf.Abs(pistonGO1.GetComponent<ElectricalComponent>().GetSimFwCurrent()) > 0.75f) && 
			(Mathf.Abs(pistonGO2.GetComponent<ElectricalComponent>().GetSimFwCurrent()) > 0.75f);
		
		textParallel.SetActive(!isOnTerminal2 && isParallel);
		
		bool isBackwards = 
			(pistonGO1.GetComponent<ElectricalComponent>().GetSimFwCurrent() < -0.1f) || 
			(pistonGO2.GetComponent<ElectricalComponent>().GetSimFwCurrent() < -0.1f);
		
		textBackwards.SetActive(!isOnTerminal2 && isBackwards && !isParallel);
		
		bool isSuccess = 
			MathUtils.FP.Feq(pistonGO1.GetComponent<ElectricalComponent>().GetSimFwCurrent(),  0.5f) && 
			MathUtils.FP.Feq(pistonGO2.GetComponent<ElectricalComponent>().GetSimFwCurrent(),  0.5f);
			
		textSuccess.SetActive(isSuccess);
		
		
		step2.SetActive(isOnTerminal2);
		
		
		
	}
}
